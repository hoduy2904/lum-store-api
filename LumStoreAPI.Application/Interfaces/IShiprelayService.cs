using LumStoreAPI.Application.DTOs.ShiprelayDTO;

namespace LumStoreAPI.Application.Interfaces;

public interface IShiprelayService
{
    /// <summary>Create a shipment in Shiprelay for a confirmed order.</summary>
    Task<ShiprelayShipmentResult> CreateShipmentAsync(ShiprelayCreateShipmentDTO dto);

    /// <summary>Update a queued/held shipment before it is processed.</summary>
    Task<ShiprelayShipmentResult> UpdateShipmentAsync(string shipmentId, ShiprelayUpdateShipmentDTO dto);

    /// <summary>Get current tracking status for a shipment.</summary>
    Task<ShiprelayTrackingResult?> GetTrackingAsync(string shipmentId);

    /// <summary>Get a paginated list of shipments from ShipRelay.</summary>
    Task<IEnumerable<ShiprelayShipmentSummaryDTO>> GetShipmentsAsync(ShiprelayGetShipmentsRequest request);

    /// <summary>Get shipping rate estimates for an order.</summary>
    Task<IEnumerable<ShiprelayRateResult>> GetRatesAsync(ShiprelayRateRequestDTO dto);

    /// <summary>Archive (cancel) a shipment — sets status to inactive.</summary>
    Task<bool> CancelShipmentAsync(string shipmentId);

    /// <summary>Restore an archived shipment back to queued status.</summary>
    Task<bool> RestoreShipmentAsync(string shipmentId);

    /// <summary>Validate incoming webhook signature from Shiprelay against the raw request body bytes.</summary>
    Task<bool> ValidateWebhookSignatureAsync(byte[] payload, string signature);

    /// <summary>Get product stock info from Shiprelay by SKU. Returns null if not found.</summary>
    Task<ShiprelayProductDTO?> GetProductBySkuAsync(string sku);
}
