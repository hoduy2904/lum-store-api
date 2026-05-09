using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace LumStoreAPI.Application.DTOs.ShiprelayDTO;

// ── Items ─────────────────────────────────────────────────────────────────

/// <summary>Line item for ShipRelay API v2 — uses ShipRelay product ID (ProductVariant.ShiprelayId).</summary>
public class ShiprelayItemDTO
{
    [Required] public int ProductId { get; set; }           // ShipRelay product ID
    [Required, Range(1, int.MaxValue)] public int Quantity { get; set; }
    [Required] public decimal Price { get; set; }
    public string? Currency { get; set; }                   // optional, defaults to USD
}

// ── Outbound: Create Shipment ──────────────────────────────────────────────

public class ShiprelayCreateShipmentDTO
{
    [Required] public int OrderId { get; set; }
    [Required, MaxLength(200)] public string OrderRef { get; set; } = default!;     // order_ref
    [Required] public decimal ShipmentTotalCost { get; set; }                        // shipment_total_cost
    public int PackageRef { get; set; } = 1;                                         // package_ref
    public DateTimeOffset ShipmentCreatedAt { get; set; } = DateTimeOffset.UtcNow;  // shipment_created_at
    public string? Type { get; set; }                                                // b2c | b2b | transfer
    public string? ShippingSelectedRef { get; set; }
    public List<string>? Tags { get; set; }

    // Address — maps to address.* in API payload
    [Required, MaxLength(150)] public string RecipientName { get; set; } = default!;
    [Required, MaxLength(500)] public string Address1 { get; set; } = default!;
    public string? Address2 { get; set; }
    [Required, MaxLength(100)] public string City { get; set; } = default!;
    [MaxLength(100)] public string? State { get; set; }     // maps to address.region
    [Required, MaxLength(20)] public string Zip { get; set; } = default!;
    [MaxLength(10)] public string Country { get; set; } = "US";
    public string? Company { get; set; }
    [MaxLength(30)] public string? Phone { get; set; }
    [MaxLength(200)] public string? Email { get; set; }
    public string? Notes { get; set; }

    public List<ShiprelayItemDTO> Items { get; set; } = [];
}

// ── Outbound: Rate Request ────────────────────────────────────────────────

public class ShiprelayRateRequestDTO
{
    [Required] public int OrderId { get; set; }
    public string? SessionId { get; set; }                  // X-Rate-Session header for cache reuse

    // Destination — maps to destination.* in API payload
    [Required, MaxLength(150)] public string RecipientName { get; set; } = default!;
    [Required, MaxLength(500)] public string Address1 { get; set; } = default!;
    public string? Address2 { get; set; }
    [Required, MaxLength(100)] public string City { get; set; } = default!;
    [Required, MaxLength(100)] public string Region { get; set; } = default!;   // state/province
    [Required, MaxLength(10)] public string Country { get; set; } = "US";
    [Required, MaxLength(20)] public string Zip { get; set; } = default!;
    public string? Company { get; set; }
    [MaxLength(30)] public string? Phone { get; set; }
    [MaxLength(200)] public string? Email { get; set; }

    public List<ShiprelayItemDTO> Items { get; set; } = [];
}

// ── Outbound: Update Shipment ─────────────────────────────────────────────

public class ShiprelayUpdateShipmentDTO
{
    [Required] public decimal ShipmentTotalCost { get; set; }
    public string? OrderRef { get; set; }
    public string? Type { get; set; }
    public string? Notes { get; set; }
    public List<string>? Tags { get; set; }
    public string? ShippingSelectedRef { get; set; }
    public List<ShiprelayItemDTO> Items { get; set; } = [];
}

// ── Outbound: List Shipments ──────────────────────────────────────────────

public class ShiprelayGetShipmentsRequest
{
    public int Page { get; set; } = 1;
    public int PerPage { get; set; } = 20;
    public string? SourceOrderId { get; set; }
    public string? SourceShipmentId { get; set; }
    public string? OrderRef { get; set; }
    public string? Status { get; set; }
    public string? TrackingNumber { get; set; }
    public string? UpdatedAtFrom { get; set; }
    public string? UpdatedAtTo { get; set; }
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
}

public class ShiprelayRateResult
{
    public string ServiceCode { get; set; } = default!;
    public string ServiceName { get; set; } = default!;
    public decimal TotalPrice { get; set; }
    public string? Description { get; set; }
    public string Currency { get; set; } = "USD";
    public DateTime? MinDeliveryDate { get; set; }
    public DateTime? MaxDeliveryDate { get; set; }
    public bool PhoneRequired { get; set; }
}

public class ShiprelayShipmentSummaryDTO
{
    public string Id { get; set; } = default!;
    public string? Status { get; set; }
    public string? OrderRef { get; set; }
    public string? SourceOrderId { get; set; }
    public string? TrackingNumber { get; set; }
    public string? TrackingUrl { get; set; }
    public string? Carrier { get; set; }
    public DateTime? UpdatedAt { get; set; }
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
    [JsonPropertyName("warehouse")]          public ShiprelayWebhookWarehouse? Warehouse { get; set; }
}

public class ShiprelayWebhookWarehouse
{
    [JsonPropertyName("id")]   public int Id { get; set; }
    [JsonPropertyName("name")] public string? Name { get; set; }
}

// ── Product / Inventory ───────────────────────────────────────────────────

public class ShiprelayProductDTO
{
    public string Sku { get; set; } = default!;
    public int AvailableStock { get; set; }
}
