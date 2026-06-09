using LumStoreAPI.Application.DTOs.IntegrationDTO;
using LumStoreAPI.Application.DTOs.ShiprelayDTO;
using LumStoreAPI.Application.Interfaces;
using LumStoreAPI.Core.Entities.Integrations;
using LumStoreAPI.Core.Interfaces.Repositories;
using LumStoreAPI.Core.Interfaces.Sytems;
using LumStoreAPI.Core.Models.Enums;

namespace LumStoreAPI.Application.Services;

public class IntegrationConfigService : IIntegrationConfigService
{
    private readonly IIntegrationConfigRepository _configRepo;
    private readonly IEventLogService _eventLog;
    private readonly IShiprelayService _shiprelayService;
    private readonly IOrderService _orderService;

    public IntegrationConfigService(
        IIntegrationConfigRepository configRepo,
        IEventLogService eventLog,
        IShiprelayService shiprelayService,
        IOrderService orderService)
    {
        _configRepo = configRepo;
        _eventLog = eventLog;
        _shiprelayService = shiprelayService;
        _orderService = orderService;
    }

    public async Task<IEnumerable<IntegrationConfigGetDTO>> GetConfigsAsync()
    {
        var configs = await _configRepo.GetConfigsAsync();
        return configs.Select(MapToDTO);
    }

    public async Task<IntegrationConfigGetDTO?> GetConfigAsync(IntegrationType type)
    {
        var config = await _configRepo.GetConfigByTypeAsync(type);
        return config == null ? null : MapToDTO(config);
    }

    public async Task<IntegrationConfigGetDTO> UpsertConfigAsync(IntegrationConfigUpsertDTO dto)
    {
        var config = new IntegrationConfig
        {
            IntegrationType = dto.IntegrationType,
            Name = dto.Name,
            BaseUrl = dto.BaseUrl,
            ApiKey = dto.ApiKey,
            ApiSecret = dto.ApiSecret,
            ResellerId = dto.ResellerId,
            WebhookSecret = dto.WebhookSecret,
            AdditionalConfig = dto.AdditionalConfig,
            IsEnabled = dto.IsEnabled
        };

        var saved = await _configRepo.UpsertConfigAsync(config);
        await _eventLog.LogInformation("IntegrationConfigService", "CONFIG_UPSERTED",
            $"Integration config {dto.IntegrationType} saved");

        return MapToDTO(saved);
    }

    public async Task<bool> DeleteConfigAsync(int configId)
    {
        var result = await _configRepo.DeleteConfigAsync(configId);
        if (result)
            await _eventLog.LogInformation("IntegrationConfigService", "CONFIG_DELETED",
                $"Integration config #{configId} deleted");
        return result;
    }

    public async Task<IEnumerable<SyncLogGetDTO>> GetSyncLogsAsync(IntegrationType? type = null)
    {
        var logs = await _configRepo.GetSyncLogsAsync(type);
        return logs.Select(MapSyncLogToDTO);
    }

    public async Task<SyncLogGetDTO> TriggerSyncAsync(IntegrationType type, int? triggeredByUserId = null)
    {
        var started = DateTimeOffset.UtcNow;
        int synced = 0, failed = 0;
        string? errorDetail = null;
        var status = SyncStatus.Success;

        if (type == IntegrationType.Shiprelay)
        {
            try
            {
                var config = await _configRepo.GetConfigByTypeAsync(type);

                // Default window: last 7 days when no previous sync recorded
                var fromDate = config?.LastSyncAt ?? DateTimeOffset.UtcNow.AddDays(-7);
                var fromDateStr = fromDate.ToString("yyyy-MM-ddTHH:mm:ssZ");

                // Paginate through all shipments updated since the last sync
                const int perPage = 50;
                int page = 1;
                bool hasMore = true;

                while (hasMore)
                {
                    var result = await _shiprelayService.GetShipmentsAsync(new ShiprelayGetShipmentsRequest
                    {
                        Page = page,
                        PerPage = perPage,
                        UpdatedAtFrom = fromDateStr
                    });

                    if (result.Data.Count == 0) break;

                    var (pageSynced, pageFailed) = await _orderService.BulkUpdateFromShipmentsAsync(result.Data);
                    synced += pageSynced;
                    failed += pageFailed;

                    hasMore = page < result.LastPage;
                    page++;
                }

                // Only advance lastSyncAt when at least partially successful
                if (synced > 0 || failed == 0)
                    await _configRepo.UpdateLastSyncAtAsync(type, started);
            }
            catch (Exception ex)
            {
                status = SyncStatus.Failed;
                errorDetail = ex.Message;
            }
        }

        if (status != SyncStatus.Failed)
        {
            if (synced > 0 && failed > 0) status = SyncStatus.Partial;
            else if (synced == 0 && failed > 0) status = SyncStatus.Failed;
        }

        var syncLog = new SyncLog
        {
            IntegrationType = type,
            SyncMode = SyncMode.Manual,
            Status = status,
            Summary = $"Manual sync for {type}: {synced} updated, {failed} failed",
            ErrorDetail = errorDetail,
            StartedAt = started,
            CompletedAt = DateTimeOffset.UtcNow,
            RecordsSynced = synced,
            RecordsFailed = failed
        };

        var log = await _configRepo.InsertSyncLogAsync(syncLog);
        await _eventLog.LogInformation("IntegrationConfigService", "SYNC_TRIGGERED",
            $"Manual sync for {type} by user {triggeredByUserId}: {synced} updated, {failed} failed");

        return MapSyncLogToDTO(log);
    }

    // ── Mappers ───────────────────────────────────────────────────────────

    private static IntegrationConfigGetDTO MapToDTO(IntegrationConfig c) => new()
    {
        ConfigId = c.ItemID,
        IntegrationType = c.IntegrationType,
        Name = c.Name,
        BaseUrl = c.BaseUrl,
        HasApiKey = !string.IsNullOrEmpty(c.ApiKey),
        HasApiSecret = !string.IsNullOrEmpty(c.ApiSecret),
        HasResellerId = !string.IsNullOrEmpty(c.ResellerId),
        HasWebhookSecret = !string.IsNullOrEmpty(c.WebhookSecret),
        IsEnabled = c.IsEnabled,
        LastSyncAt = c.LastSyncAt,
    };

    private static SyncLogGetDTO MapSyncLogToDTO(SyncLog s) => new()
    {
        LogId = s.ItemID,
        IntegrationType = s.IntegrationType,
        SyncMode = s.SyncMode,
        Status = s.Status,
        Summary = s.Summary,
        ErrorDetail = s.ErrorDetail,
        RecordsSynced = s.RecordsSynced,
        RecordsFailed = s.RecordsFailed,
        StartedAt = s.StartedAt,
        CompletedAt = s.CompletedAt,
        CreatedAt = s.CreatedAt
    };
}
