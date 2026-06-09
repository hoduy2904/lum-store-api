using LumStoreAPI.Core.Entities.Integrations;
using LumStoreAPI.Core.Models.Enums;

namespace LumStoreAPI.Core.Interfaces.Repositories;

public interface IIntegrationConfigRepository
{
    Task<IntegrationConfig?> GetConfigAsync(int configId);
    Task<IntegrationConfig?> GetConfigByTypeAsync(IntegrationType type);
    Task<IEnumerable<IntegrationConfig>> GetConfigsAsync();
    Task<IntegrationConfig> UpsertConfigAsync(IntegrationConfig config);
    Task<bool> DeleteConfigAsync(int configId);

    Task UpdateLastSyncAtAsync(IntegrationType type, DateTimeOffset syncedAt);

    Task<SyncLog> InsertSyncLogAsync(SyncLog syncLog);
    Task<IEnumerable<SyncLog>> GetSyncLogsAsync(IntegrationType? type = null, int limit = 50);
}
