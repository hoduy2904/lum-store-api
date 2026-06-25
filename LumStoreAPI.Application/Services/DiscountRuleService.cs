using LumStoreAPI.Application.DTOs.DiscountRuleDTO;
using LumStoreAPI.Application.DTOs.ProductComboDTO;
using LumStoreAPI.Application.DTOs.ProductDTO;
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
            RuleName = dto.RuleName,
            MinQuantity = dto.MinQuantity,
            MaxQuantity = dto.MaxQuantity,
            DiscountPercent = dto.DiscountPercent,
            DiscountAmount = dto.DiscountAmount,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            IsActive = dto.IsActive
        };
        var created = await _ruleRepo.InsertRuleAsync(rule, dto.ProductIds);
        return MapToDTO(created);
    }

    public async Task<DiscountRuleGetDTO> UpdateRuleAsync(int ruleId, DiscountRuleUpsertDTO dto)
    {
        var updated = await _ruleRepo.UpdateRuleAsync(ruleId, r =>
        {
            r.RuleName = dto.RuleName;
            r.MinQuantity = dto.MinQuantity;
            r.MaxQuantity = dto.MaxQuantity;
            r.DiscountPercent = dto.DiscountPercent;
            r.DiscountAmount = dto.DiscountAmount;
            r.StartDate = dto.StartDate;
            r.EndDate = dto.EndDate;
            r.IsActive = dto.IsActive;
            r.DiscountRuleMappings = dto.ProductIds.Select(x => new Core.Entities.DocumentTypes.DiscountRuleMapping { DiscountRuleId = ruleId, ProductId = x }).ToList();
        });
        return MapToDTO(updated);
    }

    public Task<bool> DeleteRuleAsync(int ruleId) => _ruleRepo.DeleteRuleAsync(ruleId);

    public async Task<Dictionary<int, IEnumerable<ProductDiscountTierDTO>>> GetDiscountTiersForProductsAsync(int[] productNodeIds)
    {
        if (productNodeIds.Length == 0) return [];

        var rules = (await _ruleRepo.GetActiveRulesBatchAsync(productNodeIds)).ToList();

        var globalTiers = rules
            .Where(r => r.DiscountRuleMappings.Count == 0)
            .Select(MapToTier)
            .ToList();

        var productSpecific = rules
            .Where(r => r.DiscountRuleMappings.Count > 0)
            .SelectMany(r => r.DiscountRuleMappings
            , (rule, mapping) => new
            {
                Rule = rule,
                ProductId = mapping.ProductId
            })
            .GroupBy(r => r.ProductId)
            .ToDictionary(g => g.Key, g => g.Select(s => MapToTier(s.Rule)).ToList());

        return productNodeIds.ToDictionary(
            nodeId => nodeId,
            nodeId =>
            {
                var tiers = new List<ProductDiscountTierDTO>(globalTiers);
                if (productSpecific.TryGetValue(nodeId, out var specific))
                    tiers.AddRange(specific);
                return (IEnumerable<ProductDiscountTierDTO>)tiers.OrderBy(t => t.MinQuantity).ToList();
            });
    }

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

        // Batch-load all rules for sub-products + combo product itself — 1 query instead of N+1
        var allNodeIds = comboItems.Select(x => x.SubProductNodeId).Append(comboProductNodeId).Distinct().ToArray();
        var rules = (await _ruleRepo.GetActiveRulesBatchAsync(allNodeIds)).ToList();

        decimal subTotal = 0;
        var itemDetails = new List<ComboItemDetailDTO>();
        int comboStock = comboItems.Count > 0 ? comboItems.Min(x => x.Stock) : 0;

        foreach (var item in comboItems)
        {
            var basePrice = item.SubProductPriceDiscount > 0 ? item.SubProductPriceDiscount : item.SubProductPrice;
            var discountedPrice = ApplyBestRule(rules, item.SubProductNodeId, basePrice, 1);
            subTotal += discountedPrice;

            itemDetails.Add(new ComboItemDetailDTO
            {
                VariantId = item.VariantId,
                VariantName = item.VariantName,
                ProductName = item.SubProductName,
                UnitPrice = basePrice,
                DiscountedPrice = discountedPrice,
                Stock = item.Stock,
                ShiprelayId = item.ShiprelayId
            });
        }

        // Apply the combo product's own discount rule on the aggregated total
        var totalPrice = ApplyBestRule(rules, comboProductNodeId, subTotal, 1);

        return new ComboPriceResult
        {
            SubTotal = subTotal,
            TotalPrice = totalPrice,
            ComboStock = comboStock,
            Items = itemDetails
        };
    }

    public async Task<Dictionary<int, ComboPriceResult>> CalculateBatchComboPricesAsync(int[] comboNodeIds)
    {
        if (comboNodeIds.Length == 0) return [];

        // 1 query: load all combo items for every combo product at once
        var allComboItems = (await _comboRepo.GetComboItemsForPricingAsync(comboNodeIds)).ToList();

        // 1 query: load all active discount rules for sub-products + combo products
        var allNodeIds = allComboItems.Select(x => x.SubProductNodeId)
            .Concat(comboNodeIds)
            .Distinct()
            .ToArray();
        var rules = allNodeIds.Length > 0
            ? (await _ruleRepo.GetActiveRulesBatchAsync(allNodeIds)).ToList()
            : [];

        var result = new Dictionary<int, ComboPriceResult>(comboNodeIds.Length);
        foreach (var comboId in comboNodeIds)
        {
            var items = allComboItems.Where(x => x.ProductId == comboId).ToList();
            decimal subTotal = 0;
            var itemDetails = new List<ComboItemDetailDTO>(items.Count);
            int comboStock = items.Count > 0 ? items.Min(x => x.Stock) : 0;

            foreach (var item in items)
            {
                var basePrice = item.SubProductPriceDiscount > 0 ? item.SubProductPriceDiscount : item.SubProductPrice;
                var discountedPrice = ApplyBestRule(rules, item.SubProductNodeId, basePrice, 1);
                subTotal += discountedPrice;

                itemDetails.Add(new ComboItemDetailDTO
                {
                    VariantId = item.VariantId,
                    VariantName = item.VariantName,
                    ProductName = item.SubProductName,
                    UnitPrice = basePrice,
                    DiscountedPrice = discountedPrice,
                    ShiprelayId = item.ShiprelayId
                });
            }

            result[comboId] = new ComboPriceResult
            {
                SubTotal = subTotal,
                TotalPrice = ApplyBestRule(rules, comboId, subTotal, 1),
                ComboStock = comboStock,
                Items = itemDetails
            };
        }

        return result;
    }

    /// <summary>
    /// Applies the best matching discount rule in-memory.
    /// Replicates GetBestRuleAsync logic without a DB round-trip.
    /// Formula: Max(0, Round(unitPrice × (1 − DiscountPercent/100) − DiscountAmount, 2))
    /// </summary>
    private static decimal ApplyBestRule(
        IEnumerable<DiscountRule> rules, int productId, decimal unitPrice, int quantity)
    {
        var best = rules
            .Where(r => (r.DiscountRuleMappings.Count == 0 || r.DiscountRuleMappings.Any(d => d.ProductId == productId)) &&
                        r.MinQuantity <= quantity &&
                        (r.MaxQuantity == null || r.MaxQuantity >= quantity))
            .OrderByDescending(r => r.DiscountPercent + r.DiscountAmount)
            .FirstOrDefault();

        if (best is null) return unitPrice;
        return Math.Max(0, Math.Round(unitPrice * (1 - best.DiscountPercent / 100) - best.DiscountAmount, 2));
    }

    private static DiscountRuleGetDTO MapToDTO(DiscountRule r) => new()
    {
        RuleId = r.ItemID,
        ProductIds = r.DiscountRuleMappings.Select(x => x.ProductId).ToArray(),
        RuleName = r.RuleName,
        MinQuantity = r.MinQuantity,
        MaxQuantity = r.MaxQuantity,
        DiscountPercent = r.DiscountPercent,
        DiscountAmount = r.DiscountAmount,
        StartDate = r.StartDate,
        EndDate = r.EndDate,
        IsActive = r.IsActive
    };

    private static ProductDiscountTierDTO MapToTier(DiscountRule r) => new()
    {
        RuleName = r.RuleName,
        MinQuantity = r.MinQuantity,
        MaxQuantity = r.MaxQuantity,
        DiscountPercent = r.DiscountPercent,
        DiscountAmount = r.DiscountAmount
    };
}
