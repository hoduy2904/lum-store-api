using LumStoreAPI.Core.Entities.Base;

namespace LumStoreAPI.Core.Entities.Integrations;

/// <summary>
/// Logged when ShipRelay accepted a shipment (HTTP 2xx) but the response body
/// could not be parsed — the shipment may exist on ShipRelay with no local ShipmentId.
/// Ops team must manually reconcile and mark IsResolved = true after recovery.
/// </summary>
public class ShiprelayReconciliationLog : BaseClassItem
{
    public int OrderId { get; set; }
    public string OrderCode { get; set; } = default!;
    public string RawResponse { get; set; } = default!;
    public bool IsResolved { get; set; }
}
