using LumStoreAPI.Core.Entities.Base;

namespace LumStoreAPI.Core.Entities.Customers;

public class LoyaltyPoint : BaseClassItem
{
    public int CustomerProfileId { get; set; }
    public int? OrderId { get; set; }
    public int Points { get; set; }            // Positive = earned, Negative = redeemed
    public string Description { get; set; } = default!;
    public DateTimeOffset? ExpiresAt { get; set; }

    public virtual CustomerProfile CustomerProfile { get; set; } = default!;
}
