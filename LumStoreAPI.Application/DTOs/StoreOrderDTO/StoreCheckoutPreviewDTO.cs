using LumStoreAPI.Application.DTOs.ShiprelayDTO;

namespace LumStoreAPI.Application.DTOs.StoreOrderDTO;

public class StoreCheckoutPreviewDTO
{
    public decimal Subtotal { get; set; }
    public decimal ShippingFee { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }
    public int ItemCount { get; set; }
    public IEnumerable<StoreCheckoutPreviewItemDTO> Items { get; set; } = [];
    public IEnumerable<ShiprelayRateResult> Rates { get; set; } = [];
}

public class StoreCheckoutPreviewItemDTO
{
    public int ProductId { get; set; }
    public int NodeId { get; set; }
    public string ProductName { get; set; } = default!;
    public string? VariantName { get; set; }
    public string? SKU { get; set; }
    public string? Image { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal { get; set; }
}
