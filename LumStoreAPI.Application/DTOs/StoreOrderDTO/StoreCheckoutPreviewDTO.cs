namespace LumStoreAPI.Application.DTOs.StoreOrderDTO;

public class StoreCheckoutPreviewDTO
{
    public decimal Subtotal { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }
    public int ItemCount { get; set; }

    /// <summary>Shipping option được chọn (theo ShippingServiceCode từ request), hoặc option đầu tiên nếu không chỉ định.</summary>
    public ShippingOptionDTO? SelectedShipping { get; set; }

    /// <summary>Toàn bộ danh sách các tùy chọn vận chuyển từ ShipRelay. Null nếu không lấy được rates.</summary>
    public IEnumerable<ShippingOptionDTO>? ShippingOptions { get; set; }

    public IEnumerable<StoreCheckoutPreviewItemDTO> Items { get; set; } = [];
}

public class ShippingOptionDTO
{
    public string ServiceCode { get; set; } = default!;
    public string ServiceName { get; set; } = default!;
    public decimal Price { get; set; }
    public string? Description { get; set; }
    public string Currency { get; set; } = "USD";
    public DateTime? MinDeliveryDate { get; set; }
    public DateTime? MaxDeliveryDate { get; set; }
}

public class StoreCheckoutPreviewItemDTO
{
    public int NodeId { get; set; }
    public string ProductName { get; set; } = default!;
    public string? VariantName { get; set; }
    public string? SKU { get; set; }
    public string? Image { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal { get; set; }
}
