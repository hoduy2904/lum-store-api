using LumStoreAPI.Core.Entities.Customers;
using LumStoreAPI.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LumStoreAPI.Infrastructure.Repositories.Presentations;

internal class DiscountRuleRepository : IDiscountRuleRepository
{
    private readonly LumStoreContext _ctx;
    public DiscountRuleRepository(LumStoreContext ctx) => _ctx = ctx;

    public Task<DiscountRule?> GetRuleAsync(int ruleId)
        => _ctx.DiscountRules.FirstOrDefaultAsync(r => r.ItemID == ruleId);

    public async Task<IEnumerable<DiscountRule>> GetRulesAsync(int? productId = null, bool activeOnly = false)
    {
        IQueryable<DiscountRule> q = _ctx.DiscountRules;
        if (productId.HasValue) q = q.Where(r => r.ProductId == null || r.ProductId == productId.Value);
        if (activeOnly)
        {
            var now = DateTimeOffset.UtcNow;
            q = q.Where(r => r.IsActive &&
                             (r.StartDate == null || r.StartDate <= now) &&
                             (r.EndDate == null || r.EndDate >= now));
        }
        return await q.OrderBy(r => r.MinQuantity).ToListAsync();
    }

    public async Task<DiscountRule> InsertRuleAsync(DiscountRule rule)
    {
        _ctx.DiscountRules.Add(rule);
        await _ctx.SaveChangesAsync();
        return rule;
    }

    public async Task<DiscountRule> UpdateRuleAsync(int ruleId, Action<DiscountRule> update)
    {
        var rule = await _ctx.DiscountRules.FindAsync(ruleId)
            ?? throw new KeyNotFoundException($"DiscountRule {ruleId} not found");
        update(rule);
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
            .Where(r => r.IsActive &&
                        (r.StartDate == null || r.StartDate <= now) &&
                        (r.EndDate == null || r.EndDate >= now) &&
                        (r.ProductId == null || r.ProductId == productId) &&
                        r.MinQuantity <= quantity &&
                        (r.MaxQuantity == null || r.MaxQuantity >= quantity))
            .OrderByDescending(r => r.DiscountAmount)
            .FirstOrDefaultAsync();
    }
}
