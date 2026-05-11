using LumStoreAPI.Application.DTOs.DiscountRuleDTO;

namespace LumStoreAPI.Application.Interfaces;

public interface IDiscountRuleService
{
    Task<IEnumerable<DiscountRuleGetDTO>> GetRulesAsync(int? productId = null, bool activeOnly = false);
    Task<DiscountRuleGetDTO?> GetRuleAsync(int ruleId);
    Task<DiscountRuleGetDTO> CreateRuleAsync(DiscountRuleUpsertDTO dto);
    Task<DiscountRuleGetDTO> UpdateRuleAsync(int ruleId, DiscountRuleUpsertDTO dto);
    Task<bool> DeleteRuleAsync(int ruleId);

    /// <summary>Calculate the best discount price for a product + quantity combo.</summary>
    Task<decimal> CalculateDiscountedPriceAsync(int productId, decimal unitPrice, int quantity);
}
