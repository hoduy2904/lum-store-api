using LumStoreAPI.Application.DTOs.IntegrationDTO;
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

    public IntegrationConfigService(IIntegrationConfigRepository configRepo, IEventLogService eventLog)
    {
        _configRepo = configRepo;
        _eventLog = eventLog;
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
        var syncLog = new SyncLog
        {
            IntegrationType = type,
            SyncMode = SyncMode.Manual,
            Status = SyncStatus.Success,
            Summary = $"Manual sync triggered for {type}",
            StartedAt = started,
            CompletedAt = DateTimeOffset.UtcNow,
            RecordsSynced = 0,
            RecordsFailed = 0
        };

        // WMS sync would be implemented via IWmsService when available
        // For now, record a sync log entry
        var log = await _configRepo.InsertSyncLogAsync(syncLog);
        await _eventLog.LogInformation("IntegrationConfigService", "SYNC_TRIGGERED",
            $"Manual sync triggered for {type} by user {triggeredByUserId}");

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
        HasWebhookSecret = !string.IsNullOrEmpty(c.WebhookSecret),
        IsEnabled = c.IsEnabled,
        LastSyncAt = c.LastSyncAt,
        ApiKey = c.ApiKey,
        SecretKey = c.ApiSecret
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
