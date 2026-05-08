using LumStoreAPI.Core.Entities.Base;
using LumStoreAPI.Core.Entities.Systems;

namespace LumStoreAPI.Core.Entities.Customers;

public class CustomerAddress : BaseClassItem
{
    public int UserId { get; set; }
    public string Phone { get; set; } = default!;
    public string Address { get; set; } = default!;
    public string City { get; set; } = default!;
    public string State { get; set; } = default!;
    public string Country { get; set; } = default!;
    public string ZipCode { get; set; } = default!;
    public string? Details { get; set; }
    public bool IsDefault { get; set; }
    public virtual User User { get; set; } = default!;
}
