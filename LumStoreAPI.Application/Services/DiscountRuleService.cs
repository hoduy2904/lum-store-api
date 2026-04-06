using LumStoreAPI.Application.DTOs.DiscountRuleDTO;
using LumStoreAPI.Application.Interfaces;
using LumStoreAPI.Core.Entities.Customers;
using LumStoreAPI.Infrastructure.Repositories.Interfaces;

namespace LumStoreAPI.Application.Services;

public class DiscountRuleService : IDiscountRuleService
{
    private readonly IDiscountRuleRepository _ruleRepo;

    public DiscountRuleService(IDiscountRuleRepository ruleRepo) => _ruleRepo = ruleRepo;

    public async Task<IEnumerable<DiscountRuleGetDTO>> GetRulesAsync(int? productId = null, bool activeOnly = false)
    {
        var rules = await _ruleRepo.GetRulesAsync(productId, activeOnly);
        return rules.Select(MapToDTO);
    }

    public async Task<DiscountRuleGetDTO?> GetRuleAsync(int ruleId)
    {
        var rule = await _ruleRepo.GetRuleAsync(ruleId);
        return rule == null ? null : MapToDTO(rule);
    }

    public async Task<DiscountRuleGetDTO> CreateRuleAsync(DiscountRuleUpsertDTO dto)
    {
        var rule = new DiscountRule
        {
            ProductId = dto.ProductId,
            VariantId = dto.VariantId,
            RuleName = dto.RuleName,
            MinQuantity = dto.MinQuantity,
            MaxQuantity = dto.MaxQuantity,
            DiscountPercent = dto.DiscountPercent,
            DiscountAmount = dto.DiscountAmount,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            IsActive = dto.IsActive
        };
        var created = await _ruleRepo.InsertRuleAsync(rule);
        return MapToDTO(created);
    }

    public async Task<DiscountRuleGetDTO> UpdateRuleAsync(int ruleId, DiscountRuleUpsertDTO dto)
    {
        var updated = await _ruleRepo.UpdateRuleAsync(ruleId, r =>
        {
            r.ProductId = dto.ProductId;
            r.VariantId = dto.VariantId;
            r.RuleName = dto.RuleName;
            r.MinQuantity = dto.MinQuantity;
            r.MaxQuantity = dto.MaxQuantity;
            r.DiscountPercent = dto.DiscountPercent;
            r.DiscountAmount = dto.DiscountAmount;
            r.StartDate = dto.StartDate;
            r.EndDate = dto.EndDate;
            r.IsActive = dto.IsActive;
        });
        return MapToDTO(updated);
    }

    public Task<bool> DeleteRuleAsync(int ruleId) => _ruleRepo.DeleteRuleAsync(ruleId);

    public async Task<decimal> CalculateDiscountedPriceAsync(int productId, int? variantId, decimal unitPrice, int quantity)
    {
        var rule = await _ruleRepo.GetBestRuleAsync(productId, variantId, quantity);
        if (rule == null) return unitPrice;

        var discounted = unitPrice;
        if (rule.DiscountPercent > 0)
            discounted -= discounted * rule.DiscountPercent / 100;
        if (rule.DiscountAmount.HasValue && rule.DiscountAmount > 0)
            discounted -= rule.DiscountAmount.Value;

        return Math.Max(0, Math.Round(discounted, 2));
    }

    private static DiscountRuleGetDTO MapToDTO(DiscountRule r) => new()
    {
        RuleId = r.ItemID,
        ProductId = r.ProductId,
        VariantId = r.VariantId,
        RuleName = r.RuleName,
        MinQuantity = r.MinQuantity,
        MaxQuantity = r.MaxQuantity,
        DiscountPercent = r.DiscountPercent,
        DiscountAmount = r.DiscountAmount,
        StartDate = r.StartDate,
        EndDate = r.EndDate,
        IsActive = r.IsActive
    };
}
