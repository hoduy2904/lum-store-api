using LumStoreAPI.Core.Entities.Base;
using LumStoreAPI.Core.Entities.DocumentEngine;

namespace LumStoreAPI.Core.Entities.Customers;

public class UserCart : BaseItem
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int NodeId { get; set; }
    public int? VariantId { get; set; }
    public int Quantity { get; set; } = 1;

    public virtual DocumentNode? Node { get; set; }
}
