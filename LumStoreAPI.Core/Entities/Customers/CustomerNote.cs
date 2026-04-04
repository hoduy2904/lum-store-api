using LumStoreAPI.Core.Entities.Base;
using LumStoreAPI.Core.Entities.Systems;

namespace LumStoreAPI.Core.Entities.Customers;

/// <summary>Internal notes added by staff about a customer.</summary>
public class CustomerNote : BaseClassItem
{
    public int CustomerProfileId { get; set; }
    public string Note { get; set; } = default!;
    public int AuthorId { get; set; }
    public string AuthorName { get; set; } = default!;

    public virtual CustomerProfile CustomerProfile { get; set; } = default!;
    public virtual User Author { get; set; } = default!;
}
