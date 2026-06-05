using LumStoreAPI.Core.Entities.Base;
using LumStoreAPI.Core.Entities.DocumentTypes;

namespace LumStoreAPI.Core.Entities.Customers;

/// <summary>Quantity-based discount rules for products.</summary>
public class DiscountRule : BaseClassItem
{
    public string RuleName { get; set; } = default!;
    public int MinQuantity { get; set; }
    public int? MaxQuantity { get; set; }
    public decimal DiscountPercent { get; set; }  // Percentage discount (0–100)
    public decimal DiscountAmount { get; set; }   // Fixed amount discount per unit
    public DateTimeOffset? StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
    public bool IsActive { get; set; } = true;
    public virtual ICollection<DiscountRuleMapping> DiscountRuleMappings { get; set; } = [];
}
