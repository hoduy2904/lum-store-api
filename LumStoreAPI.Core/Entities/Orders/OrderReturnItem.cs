using LumStoreAPI.Core.Entities.Base;

namespace LumStoreAPI.Core.Entities.Orders;

public class OrderReturnItem : BaseClassItem
{
    public int OrderReturnId { get; set; }
    public int OrderItemId { get; set; }
    public int Quantity { get; set; }
    public string? Reason { get; set; }

    public virtual OrderReturn OrderReturn { get; set; } = default!;
    public virtual OrderItem OrderItem { get; set; } = default!;
}
