using LumStoreAPI.Core.Entities.Base;

namespace LumStoreAPI.Core.Entities.Customers;

/// <summary>Quantity-based discount rules for products.</summary>
public class DiscountRule : BaseClassItem
{
    public int? ProductId { get; set; }         // null = apply to all products
    public string RuleName { get; set; } = default!;
    public int MinQuantity { get; set; }
    public int? MaxQuantity { get; set; }
    public decimal DiscountPercent { get; set; }  // Percentage discount (0–100)
    public decimal DiscountAmount { get; set; }   // Fixed amount discount per unit
    public DateTimeOffset? StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
    public bool IsActive { get; set; } = true;
}
