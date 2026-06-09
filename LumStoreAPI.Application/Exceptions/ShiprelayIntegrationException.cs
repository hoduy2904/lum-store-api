namespace LumStoreAPI.Application.Exceptions;

/// <summary>
/// Thrown when ShipRelay returned a 2xx status but the response body could not be parsed.
/// The shipment may already exist on ShipRelay — manual reconciliation is required.
/// </summary>
public class ShiprelayIntegrationException : Exception
{
    /// <summary>
    /// True when ShipRelay accepted the request (HTTP 2xx) before the failure occurred,
    /// meaning the remote resource may exist even though we have no local record of it.
    /// </summary>
    public bool ShipmentMayExistOnRemote { get; }

    /// <summary>Raw HTTP response body received from ShipRelay (for reconciliation logging).</summary>
    public string? RawResponse { get; init; }

    public ShiprelayIntegrationException(string message, bool shipmentMayExistOnRemote = false)
        : base(message)
    {
        ShipmentMayExistOnRemote = shipmentMayExistOnRemote;
    }

    public ShiprelayIntegrationException(string message, Exception innerException, bool shipmentMayExistOnRemote = false)
        : base(message, innerException)
    {
        ShipmentMayExistOnRemote = shipmentMayExistOnRemote;
    }
}
