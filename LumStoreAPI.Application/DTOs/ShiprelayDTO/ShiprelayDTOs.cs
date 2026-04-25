using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace LumStoreAPI.Application.DTOs.ShiprelayDTO;

// ── Outbound: Create Shipment ──────────────────────────────────────────────

public class ShiprelayCreateShipmentDTO
{
    [Required] public int OrderId { get; set; }

    [Required, MaxLength(150)] public string RecipientName { get; set; } = default!;
    [Required, MaxLength(500)] public string Address1 { get; set; } = default!;
    public string? Address2 { get; set; }
    [Required, MaxLength(100)] public string City { get; set; } = default!;
    [MaxLength(100)] public string? State { get; set; }
    [Required, MaxLength(20)] public string Zip { get; set; } = default!;
    [MaxLength(10)] public string Country { get; set; } = "US";

    [MaxLength(30)] public string? Phone { get; set; }
    [MaxLength(200)] public string? Email { get; set; }

    public List<ShiprelayItemDTO> Items { get; set; } = [];
    public string? ServiceCode { get; set; }    // e.g. "USPS_PRIORITY"
    public string? WarehouseId { get; set; }
    public string? Notes { get; set; }
}

public class ShiprelayItemDTO
{
    [Required] public string Sku { get; set; } = default!;
    [Required, Range(1, int.MaxValue)] public int Quantity { get; set; }
    public string? ProductName { get; set; }
}

// ── Outbound: Rate Request ────────────────────────────────────────────────

public class ShiprelayRateRequestDTO
{
    [Required] public int OrderId { get; set; }
    [Required, MaxLength(20)] public string ToZip { get; set; } = default!;
    [MaxLength(10)] public string ToCountry { get; set; } = "US";
    public List<ShiprelayItemDTO> Items { get; set; } = [];
}

// ── Inbound: Results ──────────────────────────────────────────────────────

public class ShiprelayShipmentResult
{
    public string ShipmentId { get; set; } = default!;
    public string? TrackingNumber { get; set; }
    public string? TrackingUrl { get; set; }
    public string? Carrier { get; set; }
    public string Status { get; set; } = default!;
    public decimal? ShippingCost { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}

public class ShiprelayTrackingResult
{
    public string ShipmentId { get; set; } = default!;
    public string? TrackingNumber { get; set; }
    public string? TrackingUrl { get; set; }
    public string? Carrier { get; set; }
    public string Status { get; set; } = default!;
    public string? StatusDescription { get; set; }
    public DateTimeOffset? EstimatedDelivery { get; set; }
    public DateTimeOffset? DeliveredAt { get; set; }
    public List<TrackingEventDTO> Events { get; set; } = [];
}

public class TrackingEventDTO
{
    public DateTimeOffset Timestamp { get; set; }
    public string Description { get; set; } = default!;
    public string? Location { get; set; }
}

public class ShiprelayRateResult
{
    public string ServiceCode { get; set; } = default!;
    public string ServiceName { get; set; } = default!;
    public string Carrier { get; set; } = default!;
    public decimal Price { get; set; }
    public string Currency { get; set; } = "USD";
    public int? EstimatedDays { get; set; }
}

// ── Webhook Payload ───────────────────────────────────────────────────────

public class ShiprelayWebhookPayload
{
    [JsonPropertyName("source_order_id")]    public string? SourceOrderId { get; set; }
    [JsonPropertyName("source_shipment_id")] public string? ShipmentId { get; set; }
    [JsonPropertyName("order_ref")]          public string? OrderRef { get; set; }
    [JsonPropertyName("status")]             public string? Status { get; set; }
    [JsonPropertyName("tracking_number")]    public string? TrackingNumber { get; set; }
    [JsonPropertyName("tracking_url")]       public string? TrackingUrl { get; set; }
    [JsonPropertyName("carrier")]            public string? Carrier { get; set; }
    [JsonPropertyName("service")]            public string? Service { get; set; }
}

// ── Product / Inventory ───────────────────────────────────────────────────

public class ShiprelayProductDTO
{
    public string Sku { get; set; } = default!;
    public int AvailableStock { get; set; }
}
