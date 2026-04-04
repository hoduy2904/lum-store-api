using LumStoreAPI.Core.Entities.Base;

namespace LumStoreAPI.Core.Entities.Orders;

public class OrderItem : BaseClassItem
{
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public int? VariantId { get; set; }

    public string ProductName { get; set; } = default!;
    public string? VariantName { get; set; }
    public string? SKU { get; set; }
    public string? ImageUrl { get; set; }

    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Discount { get; set; }
    public decimal Total { get; set; }

    // ── Navigation ────────────────────────────────────────────────────────
    public virtual Order Order { get; set; } = default!;
}
