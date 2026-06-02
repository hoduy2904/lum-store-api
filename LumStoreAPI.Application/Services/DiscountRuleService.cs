using LumStoreAPI.Application.DTOs.DiscountRuleDTO;
using LumStoreAPI.Application.DTOs.ProductComboDTO;
using LumStoreAPI.Application.Interfaces;
using LumStoreAPI.Core.Entities.Customers;
using LumStoreAPI.Infrastructure.Repositories.Interfaces;

namespace LumStoreAPI.Application.Services;

public class DiscountRuleService : IDiscountRuleService
{
    private readonly IDiscountRuleRepository _ruleRepo;
    private readonly IProductComboRepository _comboRepo;

    public DiscountRuleService(IDiscountRuleRepository ruleRepo, IProductComboRepository comboRepo)
    {
        _ruleRepo = ruleRepo;
        _comboRepo = comboRepo;
    }

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

    public async Task<decimal> CalculateDiscountedPriceAsync(int productId, decimal unitPrice, int quantity)
    {
        var rule = await _ruleRepo.GetBestRuleAsync(productId, quantity);
        if (rule == null) return unitPrice;

        // Formula: Max(0, Round(unitPrice × (1 - DiscountPercent/100) - DiscountAmount, 2))
        return Math.Max(0, Math.Round(unitPrice * (1 - rule.DiscountPercent / 100) - rule.DiscountAmount, 2));
    }

    public async Task<ComboPriceResult> CalculateComboPriceAsync(int comboProductNodeId)
    {
        var comboItems = (await _comboRepo.GetComboItemsForPricingAsync([comboProductNodeId])).ToList();

        decimal subTotal = 0;
        var itemDetails = new List<ComboItemDetailDTO>();

        foreach (var item in comboItems)
        {
            var basePrice = item.SubProductPriceDiscount > 0 ? item.SubProductPriceDiscount : item.SubProductPrice;
            var discountedPrice = await CalculateDiscountedPriceAsync(item.SubProductNodeId, basePrice, 1);
            subTotal += discountedPrice;

            itemDetails.Add(new ComboItemDetailDTO
            {
                VariantId = item.VariantId,
                VariantName = item.VariantName,
                ProductName = item.SubProductName,
                UnitPrice = basePrice,
                DiscountedPrice = discountedPrice
            });
        }

        // Apply the combo product's own discount rule on the aggregated total
        var totalPrice = await CalculateDiscountedPriceAsync(comboProductNodeId, subTotal, 1);

        return new ComboPriceResult
        {
            TotalPrice = totalPrice,
            Items = itemDetails
        };
    }

    private static DiscountRuleGetDTO MapToDTO(DiscountRule r) => new()
    {
        RuleId = r.ItemID,
        ProductId = r.ProductId,
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
