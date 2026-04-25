namespace LumStoreAPI.Application.DTOs.StoreOrderDTO;

public class StoreOrderSummaryDTO
{
    public int OrderId { get; set; }
    public string OrderCode { get; set; } = default!;
    public string Status { get; set; } = default!;
    public string PaymentStatus { get; set; } = default!;
    public decimal Total { get; set; }
    public int ItemCount { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public IEnumerable<OrderPreviewItemDTO> PreviewItems { get; set; } = [];
}

public class OrderPreviewItemDTO
{
    public string ProductName { get; set; } = default!;
    public string? Image { get; set; }
    public string? VariantName { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}

public class StoreOrderDetailDTO : StoreOrderSummaryDTO
{
    public string? Note { get; set; }
    public string? TrackingNumber { get; set; }
    public string? TrackingUrl { get; set; }
    public StoreOrderAddressDTO Address { get; set; } = default!;
    public IEnumerable<StoreOrderLineItemDTO> Items { get; set; } = [];
    public decimal Subtotal { get; set; }
    public decimal Shipping { get; set; }
    public decimal Tax { get; set; }
}

public class StoreOrderAddressDTO
{
    public string? Address { get; set; }
    public string? Details { get; set; }
    public string Phone { get; set; } = default!;
    public string City { get; set; } = default!;
    public string State { get; set; } = default!;
    public string ZipCode { get; set; } = default!;
    public string Country { get; set; } = default!;
}

public class StoreOrderLineItemDTO
{
    public int NodeID { get; set; }
    public string ProductName { get; set; } = default!;
    public string? Image { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public int? VariantId { get; set; }
    public string? VariantName { get; set; }
    public string? SKU { get; set; }
    public decimal LineTotal { get; set; }
}
