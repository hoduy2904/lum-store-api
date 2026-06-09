using LumStoreAPI.Core.Models.Enums;

namespace LumStoreAPI.Application.DTOs.OrderDTO;

public class OrderGetDTO
{
    public int OrderId { get; set; }
    public string OrderCode { get; set; } = default!;
    public string CustomerName { get; set; } = default!;
    public string CustomerEmail { get; set; } = default!;
    public string CustomerPhone { get; set; } = default!;
    public string ShippingAddress { get; set; } = default!;
    public string ShippingCity { get; set; } = default!;
    public string ShippingState { get; set; } = default!;
    public string ShippingZip { get; set; } = default!;
    public string ShippingCountry { get; set; } = default!;
    public decimal SubTotal { get; set; }
    public decimal ShippingFee { get; set; }
    public decimal Discount { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }
    public OrderStatus Status { get; set; }
    public string StatusName => Status.ToString();
    public PaymentStatus PaymentStatus { get; set; }
    public string? PaymentMethod { get; set; }
    public string? TrackingNumber { get; set; }
    public string? TrackingUrl { get; set; }
    public string? ShippingCarrier { get; set; }
    public string? ShippingService { get; set; }
    public string? ShippingServiceName { get; set; }
    public string? ShippingServiceDescription { get; set; }
    public DateTimeOffset? EstimatedDeliveryMin { get; set; }
    public DateTimeOffset? EstimatedDeliveryMax { get; set; }
    public string? ShiprelayShipmentId { get; set; }
    public string? CustomerNote { get; set; }
    public DateTimeOffset? ShippedAt { get; set; }
    public DateTimeOffset? DeliveredAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public List<OrderItemGetDTO> Items { get; set; } = [];
}

public class OrderItemGetDTO
{
    public int ItemId { get; set; }
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
}
