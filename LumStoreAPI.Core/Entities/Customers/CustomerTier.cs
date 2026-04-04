using LumStoreAPI.Core.Entities.Base;
using LumStoreAPI.Core.Models.Enums;

namespace LumStoreAPI.Core.Entities.Customers;

/// <summary>Admin-configurable tier thresholds.</summary>
public class CustomerTier : BaseClassItem
{
    public CustomerTierLevel TierLevel { get; set; }
    public string TierName { get; set; } = default!;
    public int MinPoints { get; set; }
    public int MaxPoints { get; set; }
    public decimal DiscountPercent { get; set; }
    public int PointsPerDollar { get; set; } = 1;   // Points earned per $1 spent
    public string? BadgeColor { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}
