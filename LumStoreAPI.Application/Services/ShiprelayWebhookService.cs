using LumStoreAPI.Application.DTOs.ShiprelayDTO;
using LumStoreAPI.Application.Interfaces;
using LumStoreAPI.Core.Entities.Orders;
using LumStoreAPI.Core.Interfaces.Sytems;
using LumStoreAPI.Core.Models.Enums;
using LumStoreAPI.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace LumStoreAPI.Application.Services;

public class ShiprelayWebhookService(
    LumStoreContext ctx,
    IEventLogService eventLog) : IShiprelayWebhookService
{
    public async Task HandleEventAsync(ShiprelayWebhookPayload payload)
    {
        var order = await ctx.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o =>
                o.ShiprelayShipmentId == payload.ShipmentId
                || o.OrderCode == payload.OrderRef);

        if (order == null)
        {
            await eventLog.LogWarning("ShiprelayWebhook", "ORDER_NOT_FOUND",
                $"No order found for ShipmentId={payload.ShipmentId}, OrderRef={payload.OrderRef}");
            return;
        }

        var newStatus = payload.Status?.ToLower() switch
        {
            "queued"     => OrderStatus.Confirmed,
            "held"       => OrderStatus.Confirmed,
            "requested"  => OrderStatus.Processing,
            "processing" => OrderStatus.Processing,
            "shipped"    => OrderStatus.Shipped,
            "in_transit" => OrderStatus.Shipped,
            "delivered"  => OrderStatus.Delivered,
            "returned"   => OrderStatus.Returned,
            "inactive"   => OrderStatus.Cancelled,
            _            => (OrderStatus?)null
        };

        if (newStatus == null || newStatus == order.Status) return;

        var prevStatus = order.Status;
        order.Status = newStatus.Value;

        if (newStatus == OrderStatus.Shipped)
        {
            order.TrackingNumber  = payload.TrackingNumber ?? order.TrackingNumber;
            order.TrackingUrl     = payload.TrackingUrl    ?? order.TrackingUrl;
            order.ShippingCarrier = payload.Carrier        ?? order.ShippingCarrier;
            order.ShippedAt       = DateTimeOffset.UtcNow;
        }

        if (newStatus == OrderStatus.Delivered)
            order.DeliveredAt = DateTimeOffset.UtcNow;

        if (string.IsNullOrEmpty(order.ShiprelayShipmentId) && !string.IsNullOrEmpty(payload.ShipmentId))
            order.ShiprelayShipmentId = payload.ShipmentId;

        ctx.OrderHistories.Add(new OrderHistory
        {
            OrderId        = order.ItemID,
            FromStatus     = prevStatus,
            ToStatus       = newStatus.Value,
            Comment        = $"ShipRelay: {payload.Status}",
            IsSystemAction = true
        });

        if (newStatus == OrderStatus.Confirmed && payload.Status?.ToLower() == "held")
            await eventLog.LogWarning("ShiprelayWebhook", "SHIPMENT_HELD",
                $"ShipRelay held shipment for OrderId={order.ItemID}, ShipmentId={payload.ShipmentId}");

        await ctx.SaveChangesAsync();

        await eventLog.LogInformation("ShiprelayWebhook", "WEBHOOK_PROCESSED",
            $"Order {order.OrderCode}: {prevStatus} → {newStatus}, ShipmentId={payload.ShipmentId}");
    }
}
