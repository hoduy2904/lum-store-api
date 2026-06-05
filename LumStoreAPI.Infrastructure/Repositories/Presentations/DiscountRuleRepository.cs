using LumStoreAPI.Core.Entities.Customers;
using LumStoreAPI.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LumStoreAPI.Infrastructure.Repositories.Presentations;

internal class DiscountRuleRepository : IDiscountRuleRepository
{
    private readonly LumStoreContext _ctx;
    public DiscountRuleRepository(LumStoreContext ctx) => _ctx = ctx;

    public Task<DiscountRule?> GetRuleAsync(int ruleId)
        => _ctx.DiscountRules.Include(x => x.DiscountRuleMappings).FirstOrDefaultAsync(r => r.ItemID == ruleId);

    public async Task<IEnumerable<DiscountRule>> GetRulesAsync(int? productId = null, bool activeOnly = false)
    {
        IQueryable<DiscountRule> q = _ctx.DiscountRules.AsNoTrackingWithIdentityResolution().Include(x => x.DiscountRuleMappings);
        if (productId.HasValue) q = q.Where(r => !r.DiscountRuleMappings.Any() || r.DiscountRuleMappings.Any(x => x.ProductId == productId.Value));
        if (activeOnly)
        {
            var now = DateTimeOffset.UtcNow;
            q = q.Where(r => r.IsActive &&
                             (r.StartDate == null || r.StartDate <= now) &&
                             (r.EndDate == null || r.EndDate >= now));
        }
        return await q.OrderBy(r => r.MinQuantity).ToListAsync();
    }

    public async Task<DiscountRule> InsertRuleAsync(DiscountRule rule, params int[] productIds)
    {
        var liveProductIds = await _ctx.Products.AsNoTracking().Select(x => x.NodeID).Where(x => productIds.Contains(x)).ToArrayAsync();
        _ctx.DiscountRules.Add(rule);
        await _ctx.SaveChangesAsync();
        if (liveProductIds.Any())
        {
            var ruleItems = liveProductIds.Select(x => new Core.Entities.DocumentTypes.DiscountRuleMapping { DiscountRuleId = rule.ItemID, ProductId = x });
            await _ctx.DiscountRuleMappings.AddRangeAsync(ruleItems);
            await _ctx.SaveChangesAsync();
            rule.DiscountRuleMappings = ruleItems.ToList();
        }
        return rule;
    }

    public async Task<DiscountRule> UpdateRuleAsync(int ruleId, Action<DiscountRule> update)
    {
        var rule = await _ctx.DiscountRules.Include(x => x.DiscountRuleMappings).FirstOrDefaultAsync(x => x.ItemID == ruleId)
            ?? throw new KeyNotFoundException($"DiscountRule {ruleId} not found");
        update(rule);
        if (rule.DiscountRuleMappings.Count > 0)
        {
            var requestedIds = rule.DiscountRuleMappings.Select(m => m.ProductId).ToArray();
            var validIds = await _ctx.Products.AsNoTracking().Select(x => x.NodeID).Where(x => requestedIds.Contains(x)).ToArrayAsync();
            rule.DiscountRuleMappings = rule.DiscountRuleMappings.Where(m => validIds.Contains(m.ProductId)).ToList();
        }
        await _ctx.SaveChangesAsync();
        return rule;
    }

    public async Task<bool> DeleteRuleAsync(int ruleId)
    {
        var rule = await _ctx.DiscountRules.FindAsync(ruleId);
        if (rule == null) return false;
        _ctx.DiscountRules.Remove(rule);
        await _ctx.SaveChangesAsync();
        return true;
    }

    public async Task<DiscountRule?> GetBestRuleAsync(int productId, int quantity)
    {
        var now = DateTimeOffset.UtcNow;
        return await _ctx.DiscountRules
            .Include(x => x.DiscountRuleMappings)
            .AsNoTrackingWithIdentityResolution()
            .Where(r => r.IsActive &&
                        (r.StartDate == null || r.StartDate <= now) &&
                        (r.EndDate == null || r.EndDate >= now) &&
                        (r.DiscountRuleMappings.Count == 0 || r.DiscountRuleMappings.Any(d => d.ProductId == productId)) &&
                        r.MinQuantity <= quantity &&
                        (r.MaxQuantity == null || r.MaxQuantity >= quantity))
            .OrderByDescending(r => r.DiscountPercent + r.DiscountAmount)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<DiscountRule>> GetActiveRulesBatchAsync(int[] productNodeIds)
    {
        var now = DateTimeOffset.UtcNow;
        return await _ctx.DiscountRules
            .Include(x => x.DiscountRuleMappings)
            .AsNoTrackingWithIdentityResolution()
            .Where(r => r.IsActive &&
                        (r.StartDate == null || r.StartDate <= now) &&
                        (r.EndDate == null || r.EndDate >= now) &&
                        (r.DiscountRuleMappings.Count == 0 || r.DiscountRuleMappings.Any(d => productNodeIds.Contains(d.ProductId))))
            .OrderBy(r => r.MinQuantity)
            .ToListAsync();
    }
}
