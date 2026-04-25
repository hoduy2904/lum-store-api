using LumStoreAPI.Application.DTOs.ShiprelayDTO;

namespace LumStoreAPI.Application.Interfaces;

public interface IShiprelayService
{
    /// <summary>Create a shipment in Shiprelay for a confirmed order.</summary>
    Task<ShiprelayShipmentResult> CreateShipmentAsync(ShiprelayCreateShipmentDTO dto);

    /// <summary>Get current tracking status for a shipment.</summary>
    Task<ShiprelayTrackingResult?> GetTrackingAsync(string shipmentId);

    /// <summary>Get shipping rate estimates for an order.</summary>
    Task<IEnumerable<ShiprelayRateResult>> GetRatesAsync(ShiprelayRateRequestDTO dto);

    /// <summary>Cancel a shipment before it is shipped.</summary>
    Task<bool> CancelShipmentAsync(string shipmentId);

    /// <summary>Validate incoming webhook signature from Shiprelay.</summary>
    bool ValidateWebhookSignature(string payload, string signature);

    /// <summary>Get product stock info from Shiprelay by SKU. Returns null if not found.</summary>
    Task<ShiprelayProductDTO?> GetProductBySkuAsync(string sku);
}
