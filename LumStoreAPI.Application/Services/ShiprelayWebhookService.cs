using LumStoreAPI.Application.DTOs.ShiprelayDTO;
using LumStoreAPI.Application.Interfaces;
using LumStoreAPI.Core.Entities.Integrations;
using LumStoreAPI.Core.Entities.Orders;
using LumStoreAPI.Core.Interfaces.Sytems;
using LumStoreAPI.Core.Models.Enums;
using LumStoreAPI.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace LumStoreAPI.Application.Services;

public class ShiprelayWebhookService(
    LumStoreContext ctx,
    IShiprelayService shiprelayService,
    IEventLogService eventLog) : IShiprelayWebhookService
{
    public async Task HandleEventAsync(ShiprelayWebhookPayload payload)
    {
        var startedAt = DateTimeOffset.UtcNow;
        var order = await ctx.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.OrderCode == payload.OrderRef);

        if (order == null)
        {
            await eventLog.LogWarning("ShiprelayWebhook", "ORDER_NOT_FOUND",
                $"No order found for ShipmentId={payload.ShipmentId}, OrderRef={payload.OrderRef}");
            AddWebhookSyncLog(
                startedAt,
                SyncStatus.Failed,
                "Shiprelay webhook order not found",
                $"ShipmentId={payload.ShipmentId}, OrderRef={payload.OrderRef}, SourceOrderId={payload.SourceOrderId}",
                recordsSynced: 0,
                recordsFailed: 1);
            await ctx.SaveChangesAsync();
            return;
        }

        var newStatus = MapStatus(payload.Status);

        if (newStatus == null)
        {
            await eventLog.LogWarning("ShiprelayWebhook", "UNSUPPORTED_STATUS",
                $"Unsupported Shiprelay status '{payload.Status}' for OrderId={order.ItemID}");
            AddWebhookSyncLog(
                startedAt,
                SyncStatus.Failed,
                "Shiprelay webhook unsupported status",
                $"OrderId={order.ItemID}, ShipmentId={payload.ShipmentId}, Status={payload.Status}",
                recordsSynced: 0,
                recordsFailed: 1);
            await ctx.SaveChangesAsync();
            return;
        }

        var prevStatus = order.Status;
        // Only allow status to move forward — ignore out-of-order webhooks
        var statusChanged = newStatus.Value > order.Status;

        if (statusChanged)
            order.Status = newStatus.Value;

        // Always capture tracking fields when present in the payload
        var trackingNumber = payload.Tracking?.TrackingNumber ?? payload.TrackingNumber;
        var trackingLink   = payload.Tracking?.TrackingLink;
        var trackingUrl    = (trackingLink is not null && trackingNumber is not null)
            ? trackingLink + trackingNumber
            : (trackingLink ?? payload.TrackingUrl);

        // Fallback: fetch tracking from ShipRelay API if payload is missing tracking data or carrier
        if (newStatus >= OrderStatus.Shipped
            && (trackingNumber is null || trackingUrl is null || payload.Carrier is null)
            && payload.ShipmentId is not null)
        {
            try
            {
                var liveTracking = await shiprelayService.GetTrackingAsync(payload.ShipmentId);
                if (liveTracking is not null)
                {
                    trackingNumber ??= liveTracking.TrackingNumber;
                    trackingUrl    ??= liveTracking.TrackingUrl;
                    if (payload.Carrier is null && liveTracking.Carrier is not null)
                        order.ShippingCarrier = liveTracking.Carrier;
                    if (payload.Service is null && liveTracking.Service is not null)
                        order.ShippingService = liveTracking.Service;
                }
            }
            catch (Exception ex)
            {
                await eventLog.LogWarning("ShiprelayWebhook", "TRACKING_FETCH_FAILED",
                    $"Could not fetch live tracking for ShipmentId={payload.ShipmentId}: {ex.Message}");
            }
        }

        if (trackingNumber is not null) order.TrackingNumber = trackingNumber;
        if (trackingUrl is not null)    order.TrackingUrl    = trackingUrl;
        if (payload.Carrier is not null) order.ShippingCarrier = payload.Carrier;
        if (payload.Service is not null) order.ShippingService = payload.Service;

        if (newStatus >= OrderStatus.Shipped)
            order.ShippedAt ??= DateTimeOffset.UtcNow;

        if (newStatus == OrderStatus.Delivered)
            order.DeliveredAt ??= DateTimeOffset.UtcNow;

        if (statusChanged)
        {
            ctx.OrderHistories.Add(new OrderHistory
            {
                OrderId = order.ItemID,
                FromStatus = prevStatus,
                ToStatus = newStatus.Value,
                Comment = $"ShipRelay: {payload.Status}",
                IsSystemAction = true
            });
        }

        if (newStatus == OrderStatus.Confirmed && payload.Status?.ToLowerInvariant() == "held")
            await eventLog.LogWarning("ShiprelayWebhook", "SHIPMENT_HELD",
                $"ShipRelay held shipment for OrderId={order.ItemID}, ShipmentId={payload.ShipmentId}");

        AddWebhookSyncLog(
            startedAt,
            SyncStatus.Success,
            "Shiprelay webhook processed",
            $"OrderId={order.ItemID}, OrderCode={order.OrderCode}, ShipmentId={payload.ShipmentId}, Status={payload.Status}, InternalStatus={newStatus}",
            recordsSynced: 1,
            recordsFailed: 0);

        await ctx.SaveChangesAsync();

        await eventLog.LogInformation("ShiprelayWebhook", "WEBHOOK_PROCESSED",
            $"Order {order.OrderCode}: {prevStatus} -> {newStatus}, ShipmentId={payload.ShipmentId}");
    }

    private static OrderStatus? MapStatus(string? status)
        => status?.ToLowerInvariant() switch
        {
            "queued" => OrderStatus.Confirmed,
            "held" => OrderStatus.Confirmed,
            "requested" => OrderStatus.Processing,
            "processing" => OrderStatus.Processing,
            "shipped" => OrderStatus.Shipped,
            "in_transit" => OrderStatus.Shipped,
            "delivered" => OrderStatus.Delivered,
            "returned" => OrderStatus.Returned,
            "inactive" => OrderStatus.Cancelled,
            _ => null
        };

    private void AddWebhookSyncLog(
        DateTimeOffset startedAt,
        SyncStatus status,
        string summary,
        string? errorDetail,
        int recordsSynced,
        int recordsFailed)
    {
        ctx.SyncLogs.Add(new SyncLog
        {
            IntegrationType = IntegrationType.Shiprelay,
            SyncMode = SyncMode.Webhook,
            Status = status,
            Summary = summary,
            ErrorDetail = errorDetail,
            RecordsSynced = recordsSynced,
            RecordsFailed = recordsFailed,
            StartedAt = startedAt,
            CompletedAt = DateTimeOffset.UtcNow
        });
    }
}
