using LumStoreAPI.Application.DTOs.ShiprelayDTO;

namespace LumStoreAPI.Application.Interfaces;

public interface IShiprelayWebhookService
{
    /// <summary>Process an inbound Shiprelay webhook event payload.</summary>
    Task HandleEventAsync(ShiprelayWebhookPayload payload);
}
