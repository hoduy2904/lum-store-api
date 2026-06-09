namespace LumStoreAPI.Application.DTOs.StoreOrderDTO;

public class StoreOrderTrackingDTO
{
    public string? TrackingNumber { get; set; }
    public string? TrackingUrl { get; set; }
    public string? Carrier { get; set; }
    public string? Status { get; set; }
    public DateTimeOffset? ShippedAt { get; set; }
    public DateTimeOffset? DeliveredAt { get; set; }
}
