using LumStoreAPI.Core.Entities.Base;
using LumStoreAPI.Core.Entities.Systems;
using LumStoreAPI.Core.Models.Enums;

namespace LumStoreAPI.Core.Entities.Orders;

/// <summary>Audit trail for order status transitions.</summary>
public class OrderHistory : BaseClassItem
{
    public int OrderId { get; set; }
    public OrderStatus FromStatus { get; set; }
    public OrderStatus ToStatus { get; set; }
    public string? Comment { get; set; }
    public int? ChangedByUserId { get; set; }
    public string? ChangedByName { get; set; }
    public bool IsSystemAction { get; set; }

    public virtual Order Order { get; set; } = default!;
    public virtual User? ChangedBy { get; set; }
}
