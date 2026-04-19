using LumStoreAPI.Core.Entities.Base;
using LumStoreAPI.Core.Entities.Systems;
using LumStoreAPI.Core.Models.Enums;

namespace LumStoreAPI.Core.Entities.Customers;

/// <summary>Extended customer data linked to a User account.</summary>
public class CustomerProfile : BaseClassItem
{
    public int UserId { get; set; }
    public CustomerTierLevel TierLevel { get; set; } = CustomerTierLevel.Standard;
    public int TotalPoints { get; set; }
    public int AvailablePoints { get; set; }
    public decimal TotalSpent { get; set; }
    public int TotalOrders { get; set; }
    public string Phone { get; set; } = default!;
    public DateTimeOffset? Birthday { get; set; }

    public virtual User User { get; set; } = default!;
    public virtual ICollection<LoyaltyPoint> LoyaltyPoints { get; set; } = [];
    public virtual ICollection<CustomerNote> CustomerNotes { get; set; } = [];
    public virtual ICollection<CustomerAddress> CustomerAddresses { get; set; } = [];
}
