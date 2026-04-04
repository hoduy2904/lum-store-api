using LumStoreAPI.Application.DTOs.IntegrationDTO;
using LumStoreAPI.Core.Models.Enums;

namespace LumStoreAPI.Application.Interfaces;

public interface IIntegrationConfigService
{
    Task<IEnumerable<IntegrationConfigGetDTO>> GetConfigsAsync();
    Task<IntegrationConfigGetDTO?> GetConfigAsync(IntegrationType type);
    Task<IntegrationConfigGetDTO> UpsertConfigAsync(IntegrationConfigUpsertDTO dto);
    Task<bool> DeleteConfigAsync(int configId);
    Task<IEnumerable<SyncLogGetDTO>> GetSyncLogsAsync(IntegrationType? type = null);

    /// <summary>Manually trigger a sync for the given integration type.</summary>
    Task<SyncLogGetDTO> TriggerSyncAsync(IntegrationType type, int? triggeredByUserId = null);
}
