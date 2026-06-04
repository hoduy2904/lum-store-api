using LumStoreAPI.Core.Entities.Customers;

namespace LumStoreAPI.Infrastructure.Repositories.Interfaces;

public interface IDiscountRuleRepository
{
    Task<DiscountRule?> GetRuleAsync(int ruleId);
    Task<IEnumerable<DiscountRule>> GetRulesAsync(int? productId = null, bool activeOnly = false);
    Task<DiscountRule> InsertRuleAsync(DiscountRule rule);
    Task<DiscountRule> UpdateRuleAsync(int ruleId, Action<DiscountRule> update);
    Task<bool> DeleteRuleAsync(int ruleId);

    /// <summary>Get the best applicable discount for a product + quantity combo.</summary>
    Task<DiscountRule?> GetBestRuleAsync(int productId, int quantity);

    /// <summary>Get all active rules for a batch of product node IDs (including global rules where ProductId is null).</summary>
    Task<IEnumerable<DiscountRule>> GetActiveRulesBatchAsync(int[] productNodeIds);
}
