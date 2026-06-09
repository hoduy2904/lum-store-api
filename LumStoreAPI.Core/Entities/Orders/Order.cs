using LumStoreAPI.Core.Entities.Base;
using LumStoreAPI.Core.Entities.Customers;
using LumStoreAPI.Core.Entities.Systems;
using LumStoreAPI.Core.Models.Enums;

namespace LumStoreAPI.Core.Entities.Orders;

public class Order : BaseClassItem
{
    public string OrderCode { get; set; } = default!;

    // ── Customer ──────────────────────────────────────────────────────────
    public int? CustomerId { get; set; }
    public string CustomerName { get; set; } = default!;
    public string CustomerEmail { get; set; } = default!;
    public string CustomerPhone { get; set; } = default!;

    // ── Shipping address ──────────────────────────────────────────────────
    public string ShippingAddress { get; set; } = default!;
    public string? ShippingDetails { get; set; }
    public string ShippingCity { get; set; } = default!;
    public string ShippingState { get; set; } = default!;
    public string ShippingZip { get; set; } = default!;
    public string ShippingCountry { get; set; } = "US";

    // ── Financials ────────────────────────────────────────────────────────
    public decimal SubTotal { get; set; }
    public decimal ShippingFee { get; set; }
    public decimal Discount { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }

    // ── Status ────────────────────────────────────────────────────────────
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unpaid;
    public string? PaymentMethod { get; set; }

    // ── Shiprelay ─────────────────────────────────────────────────────────
    public string? ShiprelayShipmentId { get; set; }
    public string? TrackingNumber { get; set; }
    public string? TrackingUrl { get; set; }
    public string? ShippingCarrier { get; set; }
    public string? ShippingService { get; set; }
    public DateTimeOffset? ShippedAt { get; set; }
    public DateTimeOffset? DeliveredAt { get; set; }
    /// <summary>
    /// Set to true when ShipRelay accepted the shipment (HTTP 2xx) but returned
    /// an unparseable response. A ShiprelayReconciliationLog entry is also written.
    /// Ops team must resolve manually and reset to false after confirming the ShipmentId.
    /// </summary>
    public bool NeedsShiprelayReconciliation { get; set; }

    // ── Notes ─────────────────────────────────────────────────────────────
    public string? CustomerNote { get; set; }

    // ── Navigation ────────────────────────────────────────────────────────
    public virtual CustomerProfile? Customer { get; set; }
    public virtual ICollection<OrderItem> OrderItems { get; set; } = [];
    public virtual ICollection<OrderHistory> OrderHistories { get; set; } = [];
    public virtual ICollection<OrderNote> OrderNotes { get; set; } = [];
    public virtual ICollection<OrderReturn> OrderReturns { get; set; } = [];
}
