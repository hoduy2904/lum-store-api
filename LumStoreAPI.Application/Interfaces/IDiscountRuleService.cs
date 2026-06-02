using LumStoreAPI.Application.DTOs.DiscountRuleDTO;
using LumStoreAPI.Application.DTOs.ProductComboDTO;

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

    /// <summary>
    /// Calculate combo product price by summing each sub-product's discounted price,
    /// then applying the combo's own discount rule on the total.
    /// </summary>
    Task<ComboPriceResult> CalculateComboPriceAsync(int comboProductNodeId);
}
