using LumStoreAPI.Core.Entities.Base;
using LumStoreAPI.Core.Entities.DocumentEngine;

namespace LumStoreAPI.Core.Entities.Customers;

public class UserWishlist : BaseItem
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int NodeId { get; set; }

    public virtual DocumentNode? Node { get; set; }
}
