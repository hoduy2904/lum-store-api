using LumStoreAPI.Core.Entities.Base;
using LumStoreAPI.Core.Entities.Systems;
using LumStoreAPI.Core.Models.Enums;

namespace LumStoreAPI.Core.Entities.Orders;

public class OrderReturn : BaseClassItem
{
    public int OrderId { get; set; }
    public string Reason { get; set; } = default!;
    public ReturnStatus Status { get; set; } = ReturnStatus.Pending;
    public long RefundAmount { get; set; }
    public string? AdminNote { get; set; }
    public int? ReviewedByUserId { get; set; }
    public string? StripeRefundId { get; set; }
    public DateTimeOffset? ReviewedAt { get; set; }

    public virtual Order Order { get; set; } = default!;
    public virtual User? ReviewedBy { get; set; }
    public virtual ICollection<OrderReturnItem> ReturnItems { get; set; } = [];
}
