using LumStoreAPI.Application.Interfaces;
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

            // Stock pull every 30 minutes
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

                        var buildRequest = await BuildRequest(variant);
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

                        buildRequest = await BuildRequest(variant);
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
        await using var scope = _serviceProvider.CreateAsyncScope();
        var ctx = scope.ServiceProvider.GetRequiredService<LumStoreContext>();
        var productService = scope.ServiceProvider.GetRequiredService<IShiprelayProductService>();

        var variants = await ctx.ProductVariants
            .Where(v => v.ShiprelayId > 0)
            .ToListAsync(ct);

        if (variants.Count == 0) return;

        int synced = 0;
        foreach (var variant in variants)
        {
            if (ct.IsCancellationRequested) break;

            var exists = await productService.IsExistsProductAsync(variant.ShiprelayId);
            if (!exists) continue;

            var result = await productService.GetShiprelayProductsAsync(
                new ShiprelayProductGetRequest { SKU = variant.SKU, PerPage = 1 });

            var product = result?.Data?.FirstOrDefault();
            if (product is null) continue;

            variant.Stock = product.StockCount ?? 0;
            synced++;
        }

        if (synced > 0)
            await ctx.SaveChangesAsync(ct);

        _logger.LogInformation("ShipRelay stock sync: {Synced}/{Total} variants updated", synced, variants.Count);
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private async Task<ShiprelayProductUpdateRequest> BuildRequest(Core.Entities.DocumentTypes.ProductVariant variant)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var mediaService = scope.ServiceProvider.GetRequiredService<IMediaService>();
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
                ShipWeight = product?.Weight ?? 0,
                ShipLength = product?.Length ?? 0,
                ShipWidth = product?.Width ?? 0,
                ShipHeight = product?.Height ?? 0,
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
