using LumStoreAPI.Core.Entities.Integrations;
using LumStoreAPI.Core.Models.Enums;
using LumStoreAPI.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LumStoreAPI.Infrastructure.Repositories.Presentations;

internal class IntegrationConfigRepository : IIntegrationConfigRepository
{
    private readonly LumStoreContext _ctx;
    public IntegrationConfigRepository(LumStoreContext ctx) => _ctx = ctx;

    public Task<IntegrationConfig?> GetConfigAsync(int configId)
        => _ctx.IntegrationConfigs.FirstOrDefaultAsync(c => c.ItemID == configId);

    public Task<IntegrationConfig?> GetConfigByTypeAsync(IntegrationType type)
        => _ctx.IntegrationConfigs.FirstOrDefaultAsync(c => c.IntegrationType == type && c.IsEnabled);

    public async Task<IEnumerable<IntegrationConfig>> GetConfigsAsync()
        => await _ctx.IntegrationConfigs.OrderBy(c => c.IntegrationType).ToListAsync();

    public async Task<IntegrationConfig> UpsertConfigAsync(IntegrationConfig config)
    {
        var existing = await _ctx.IntegrationConfigs
            .FirstOrDefaultAsync(c => c.IntegrationType == config.IntegrationType);

        if (existing == null)
        {
            _ctx.IntegrationConfigs.Add(config);
            await _ctx.SaveChangesAsync();
            return config;
        }

        existing.Name = config.Name;
        existing.BaseUrl = config.BaseUrl;
        existing.ApiKey = config.ApiKey;
        existing.ApiSecret = config.ApiSecret;
        existing.WebhookSecret = config.WebhookSecret;
        existing.AdditionalConfig = config.AdditionalConfig;
        existing.IsEnabled = config.IsEnabled;
        await _ctx.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteConfigAsync(int configId)
    {
        var config = await _ctx.IntegrationConfigs.FindAsync(configId);
        if (config == null) return false;
        _ctx.IntegrationConfigs.Remove(config);
        await _ctx.SaveChangesAsync();
        return true;
    }

    public async Task<SyncLog> InsertSyncLogAsync(SyncLog syncLog)
    {
        _ctx.SyncLogs.Add(syncLog);
        await _ctx.SaveChangesAsync();
        return syncLog;
    }

    public async Task<IEnumerable<SyncLog>> GetSyncLogsAsync(IntegrationType? type = null, int limit = 50)
    {
        IQueryable<SyncLog> q = _ctx.SyncLogs.OrderByDescending(s => s.CreatedAt);
        if (type.HasValue) q = q.Where(s => s.IntegrationType == type.Value);
        return await q.Take(limit).ToListAsync();
    }
}
