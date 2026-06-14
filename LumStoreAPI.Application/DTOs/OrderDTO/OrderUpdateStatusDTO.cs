using LumStoreAPI.Core.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace LumStoreAPI.Application.DTOs.OrderDTO;

public class OrderUpdateStatusDTO
{
    [Required]
    public OrderStatus NewStatus { get; set; }

    public PaymentStatus? NewPaymentStatus { get; set; }

    [MaxLength(500)]
    public string? Comment { get; set; }
    public string? StripePaymentIntentId { get; set; }
    public string? PaymentMethod { get; set; }
}

public class OrderUpdateTrackingDTO
{
    [MaxLength(200)]
    public string? TrackingNumber { get; set; }

    [MaxLength(500)]
    public string? TrackingUrl { get; set; }

    [MaxLength(100)]
    public string? ShippingCarrier { get; set; }

    [MaxLength(100)]
    public string? ShippingService { get; set; }

    [MaxLength(200)]
    public string? ShiprelayShipmentId { get; set; }
}
