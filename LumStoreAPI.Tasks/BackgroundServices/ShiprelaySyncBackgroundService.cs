using LumStoreAPI.Application.Interfaces;
using LumStoreAPI.Core.Interfaces.Repositories;
using LumStoreAPI.Core.Interfaces.Sytems;
using LumStoreAPI.Core.Models.Enums;
using LumStoreAPI.Infrastructure;
using LumStoreAPI.SDK.Shiprelay.Interfaces;
using LumStoreAPI.SDK.Shiprelay.Models.Product;
using LumStoreAPI.SDK.Shiprelay.Models.Requests;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LumStoreAPI.Tasks.BackgroundServices;

public class ShiprelaySyncBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ShiprelaySyncBackgroundService> _logger;

    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan StockSyncEvery = TimeSpan.FromMinutes(30);

    private DateTimeOffset _lastStockSync = DateTimeOffset.MinValue;

    public ShiprelaySyncBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<ShiprelaySyncBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(PollInterval, stoppingToken);

            try
            {
                await ProcessQueueAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "ShipRelay queue processing failed");
            }

            // Stock pull every 1 minutes
            if (DateTimeOffset.UtcNow - _lastStockSync >= StockSyncEvery)
            {
                try
                {
                    await SyncStockFromShiprelayAsync(stoppingToken);
                    _lastStockSync = DateTimeOffset.UtcNow;
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogError(ex, "ShipRelay stock sync failed");
                }
            }
        }
    }

    // ── Push: Local → ShipRelay ───────────────────────────────────────────

    private async Task ProcessQueueAsync(CancellationToken ct)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var ctx = scope.ServiceProvider.GetRequiredService<LumStoreContext>();
        var productService = scope.ServiceProvider.GetRequiredService<IShiprelayProductService>();
        var eventLog = scope.ServiceProvider.GetRequiredService<IEventLogService>();
        var mediaService = scope.ServiceProvider.GetRequiredService<IMediaService>();

        // Take up to 20 pending items per cycle
        var pending = await ctx.ShiprelayDataSyncs
            .Where(s => s.Status != EmailStatus.Success)
            .OrderBy(s => s.Status)
            .ThenBy(x => x.CreatedAt)
            .Take(20)
            .ToListAsync(ct);

        if (pending.Count == 0) return;

        var variantIds = pending.Select(s => s.VariantID).Distinct().ToArray();

        var variants = await ctx.ProductVariants
            .Include(x => x.CasePack)
            .Where(v => variantIds.Contains(v.ItemID))
            .ToListAsync(ct);

        // ProductVariant.Product is ignored by EF (NodeID-based join) — load separately
        var productNodeIds = variants.Select(v => v.ProductID).Distinct().ToArray();
        var products = await ctx.Products
            .Where(p => productNodeIds.Contains(p.NodeID))
            .ToListAsync(ct);

        foreach (var v in variants)
            v.Product = products.FirstOrDefault(p => p.NodeID == v.ProductID);

        foreach (var syncEntry in pending)
        {
            if (ct.IsCancellationRequested) break;

            syncEntry.RunnedAt = DateTimeOffset.UtcNow;

            try
            {
                var variant = variants
                .FirstOrDefault(v => v.ItemID == syncEntry.VariantID);

                switch (syncEntry.EntryActionStatus)
                {
                    case EntryActionStatus.INSERT:
                        if (variant is null) { MarkFailed(syncEntry, "Variant not found"); break; }

                        if ((variant.Product?.ProductType ?? ProductType.SIMPLE) == ProductType.BUNDLE)
                        {
                            _logger.LogWarning("Skipping ShipRelay sync for variant {VariantId} (SKU={SKU}): BUNDLE type is not supported by ShipRelay. Assign a SIMPLE, PACKING, or CASEPACK ProductType before syncing.", variant.ItemID, variant.SKU);
                            await eventLog.LogWarning("ShiprelaySyncService", "SHIPRELAY_BUNDLE_SKIPPED",
                                $"Skipping sync for VariantId={variant.ItemID} SKU={variant.SKU}: BUNDLE products cannot be synced to ShipRelay.");
                            syncEntry.Message = "Skipped: BUNDLE products are not supported by ShipRelay.";
                            break;
                        }

                        var buildRequest = await BuildRequest(variant, mediaService, onceReceived: false);
                        var created = await productService.PostProductAsync(
                            buildRequest, variant.Product?.ProductType ?? ProductType.SIMPLE);

                        if (created is not null)
                        {
                            variant.ShiprelayId = created.Id;
                            MarkSuccess(syncEntry);
                            await eventLog.LogInformation("ShiprelaySyncService", "SHIPRELAY_PRODUCT_CREATED",
                                $"POST products OK | VariantId={variant.ItemID} | SKU={variant.SKU} | ShiprelayId={created.Id}");
                        }
                        else
                        {
                            MarkFailed(syncEntry, "ShipRelay returned null on insert");
                            await eventLog.LogWarning("ShiprelaySyncService", "SHIPRELAY_PRODUCT_CREATE_FAILED",
                                $"POST products returned null | VariantId={variant.ItemID} | SKU={variant.SKU}");
                        }
                        break;

                    case EntryActionStatus.UPDATE:
                        if (variant is null) { MarkFailed(syncEntry, "Variant not found"); break; }

                        if ((variant.Product?.ProductType ?? ProductType.SIMPLE) == ProductType.BUNDLE)
                        {
                            _logger.LogWarning("Skipping ShipRelay sync for variant {VariantId} (SKU={SKU}): BUNDLE type is not supported by ShipRelay. Assign a SIMPLE, PACKING, or CASEPACK ProductType before syncing.", variant.ItemID, variant.SKU);
                            await eventLog.LogWarning("ShiprelaySyncService", "SHIPRELAY_BUNDLE_SKIPPED",
                                $"Skipping sync for VariantId={variant.ItemID} SKU={variant.SKU}: BUNDLE products cannot be synced to ShipRelay.");
                            syncEntry.Message = "Skipped: BUNDLE products are not supported by ShipRelay.";
                            break;
                        }

                        if (variant.ShiprelayOnceReceived)
                            _logger.LogInformation("Product {SKU}: once_received=true, skipping dimension fields in update request", variant.SKU);

                        buildRequest = await BuildRequest(variant, mediaService, onceReceived: variant.ShiprelayOnceReceived);
                        if (variant.ShiprelayId == 0)
                        {
                            var upserted = await productService.PostProductAsync(
                                buildRequest, variant.Product?.ProductType ?? ProductType.SIMPLE);
                            if (upserted is not null)
                            {
                                variant.ShiprelayId = upserted.Id;
                                await eventLog.LogInformation("ShiprelaySyncService", "SHIPRELAY_PRODUCT_UPSERTED",
                                    $"POST products (upsert) OK | VariantId={variant.ItemID} | SKU={variant.SKU} | ShiprelayId={upserted.Id}");
                            }
                        }
                        else
                        {
                            await productService.UpdateProductAsync(
                                variant.ShiprelayId, buildRequest,
                                variant.Product?.ProductType ?? ProductType.SIMPLE,
                                ensureSuccess: false);
                            await eventLog.LogInformation("ShiprelaySyncService", "SHIPRELAY_PRODUCT_UPDATED",
                                $"PUT products/{variant.ShiprelayId} OK | VariantId={variant.ItemID} | SKU={variant.SKU}");
                        }
                        MarkSuccess(syncEntry);
                        break;

                    case EntryActionStatus.DELETE:

                        if (syncEntry.ShiprelayId > 0)
                        {
                            await productService.ArchiveProductAsync(syncEntry.ShiprelayId.Value);
                            await eventLog.LogInformation("ShiprelaySyncService", "SHIPRELAY_PRODUCT_ARCHIVED",
                                $"PATCH products/{syncEntry.ShiprelayId.Value}/archive OK | VariantId={syncEntry.VariantID}");
                        }
                        MarkSuccess(syncEntry);
                        break;
                }

                ctx.ShiprelayDataSyncs.Remove(syncEntry);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ShipRelay sync failed for VariantID={VariantID}", syncEntry.VariantID);
                MarkFailed(syncEntry, ex.Message);
                await eventLog.LogException("ShiprelaySyncService", "SHIPRELAY_SYNC_EXCEPTION",
                    $"Sync exception | VariantId={syncEntry.VariantID} | Action={syncEntry.EntryActionStatus}", ex);
            }
        }

        await ctx.SaveChangesAsync(ct);

        var failed = pending.Count(s => s.Status == EmailStatus.Failed);
        var success = pending.Count(s => s.Status == EmailStatus.Success);
        _logger.LogInformation("ShipRelay queue processed: {Success} ok, {Failed} failed", success, failed);

        if (failed > 0)
            await eventLog.LogWarning("ShiprelaySyncService", "SHIPRELAY_QUEUE_PARTIAL",
                $"Queue batch: {success} succeeded, {failed} failed");
    }

    // ── Pull: ShipRelay → Local (stock cache) ────────────────────────────

    private async Task SyncStockFromShiprelayAsync(CancellationToken ct)
    {
        DateTime currentTime = DateTime.UtcNow;
        await using var scope = _serviceProvider.CreateAsyncScope();
        var ctx = scope.ServiceProvider.GetRequiredService<LumStoreContext>();
        var productService = scope.ServiceProvider.GetRequiredService<IShiprelayProductService>();
        var keySettingService = scope.ServiceProvider.GetRequiredService<ISettingKeyValueRepository>();

        var lastSyncedData = await keySettingService.GetSettingKeyAsync("SHIPRELAY_STOCK_LAST_SYNC");
        if (!DateTime.TryParse(lastSyncedData?.SettingValue, out DateTime lastSynced))
        {
            lastSynced = new DateTime(2026, 01, 01).ToUniversalTime();
        }
        var updateVariants = await productService.GetShiprelayProductsAsync(
            // ToUniversalTime() ensures Kind=UTC regardless of how lastSynced was parsed from storage.
            new ShiprelayProductGetRequest { Page = 1, PerPage = 10000, UpdatedAtFrom = lastSynced.ToUniversalTime() }
        );

        if (updateVariants is null || !updateVariants.Data.Any()) return;
        var updateVariantDict = updateVariants.Data.ToDictionary(x => x.Id, x => x) ?? [];

        var productVariants = await ctx.ProductVariants
            .Where(x => x.ShiprelayId != 0 && updateVariantDict.Keys.Contains(x.ShiprelayId))
            .ToArrayAsync();

        if (!productVariants.Any()) return;

        foreach (var variant in productVariants)
        {
            if (!updateVariantDict.ContainsKey(variant.ShiprelayId)) continue;
            variant.Stock = updateVariantDict[variant.ShiprelayId].StockCount ?? 0;
            variant.ShiprelayOnceReceived = updateVariantDict[variant.ShiprelayId].OnceReceived;
        }

        int changeCounter = 0;

        if (ctx.ChangeTracker.HasChanges())
            changeCounter = await ctx.SaveChangesAsync(ct);

        if (lastSyncedData is null)
        {
            await keySettingService.InsertSettingKeyAsync(new Core.Entities.Systems.SettingKeyValue
            {
                SettingCode = "SHIPRELAY_STOCK_LAST_SYNC",
                SettingName = "Shiprelay stock last synced",
                SettingValue = DateTime.UtcNow.ToString()
            });
        }
        else
        {
            lastSyncedData.SettingValue = currentTime.ToString();
            await keySettingService.UpdateSettingKeyAsync("SHIPRELAY_STOCK_LAST_SYNC", lastSyncedData);
        }

        _logger.LogInformation("ShipRelay stock sync: {Synced}/{Total} variants updated", changeCounter, productVariants.Length);
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private async Task<ShiprelayProductUpdateRequest> BuildRequest(
        Core.Entities.DocumentTypes.ProductVariant variant,
        IMediaService mediaService,
        bool onceReceived = false)
    {
        var product = variant.Product;

        var imageGuids = variant.Images;
        if (!imageGuids.Any())
        {
            imageGuids = product?.Images ?? [];
        }

        var media = await mediaService.GetMediaItemsAsync([imageGuids.FirstOrDefault()]);
        return new ShiprelayProductUpdateRequest
        {
            SourceId = variant.ItemID.ToString(),
            SKU = variant.SKU,
            Barcode = variant.UPC,
            Name = product is not null
                ? $"{product.ProductName} - {variant.VariantName}"
                : variant.VariantName,
            Category = Enum.GetName(product?.ProductGroup ?? ProductGroup.HARD_GOODS)?.ToLowerInvariant().Replace('_', '-')!,
            Settings = new ShiprelayProductSetting
            {
                // Dimension fields are omitted (null) when once_received=true — ShipRelay rejects changes after first stock receipt.
                ShipWeight = onceReceived ? null : product?.Weight ?? 0,
                ShipLength = onceReceived ? null : product?.Length ?? 0,
                ShipWidth  = onceReceived ? null : product?.Width ?? 0,
                ShipHeight = onceReceived ? null : product?.Height ?? 0,
                IsFragile = product?.IsFragile ?? false,
                IsFoldable = product?.IsFoldable ?? false,
                IsAlcoholic = product?.IsAlcoholic ?? false,
                IsHazmat = product?.IsHazmat ?? false,
                NeedsBox = product?.IsNeedBox ?? false,
                ParentQty = product?.ProductType == ProductType.CASEPACK ? product.ParentQty : null
            },
            Thumb = media.FirstOrDefault()?.FileURL ?? string.Empty,
            ParentId = variant.CasePack?.ShiprelayId,
        };
    }

    private void MarkSuccess(Core.Entities.Integrations.ShiprelayDataSync entry)
        => entry.Status = EmailStatus.Success;

    private void MarkFailed(Core.Entities.Integrations.ShiprelayDataSync entry, string message)
    {
        entry.Status = EmailStatus.Failed;
        entry.Message = message;
    }
}
