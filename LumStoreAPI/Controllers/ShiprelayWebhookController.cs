using System.Text;
using System.Text.Json;
using LumStoreAPI.Application.Interfaces;
using LumStoreAPI.Application.DTOs.ShiprelayDTO;
using LumStoreAPI.Core.Entities.Orders;
using LumStoreAPI.Core.Interfaces.Sytems;
using LumStoreAPI.Core.Models.Enums;
using LumStoreAPI.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LumStoreAPI.Controllers;

/// <summary>Receives inbound webhook events from Shiprelay.</summary>
[Route("api/webhooks/shiprelay")]
[ApiController]
[AllowAnonymous]
public class ShiprelayWebhookController : ControllerBase
{
    private readonly IShiprelayService _shiprelayService;
    private readonly LumStoreContext _ctx;
    private readonly IEventLogService _eventLog;

    public ShiprelayWebhookController(
        IShiprelayService shiprelayService,
        LumStoreContext ctx,
        IEventLogService eventLog)
    {
        _shiprelayService = shiprelayService;
        _ctx = ctx;
        _eventLog = eventLog;
    }

    /// <summary>POST /api/webhooks/shiprelay — Receive Shiprelay events.</summary>
    [HttpPost]
    public async Task<IActionResult> Receive()
    {
        Request.EnableBuffering();
        using var reader = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true);
        var rawBody = await reader.ReadToEndAsync();
        Request.Body.Position = 0;

        var signature = Request.Headers["X-Shiprelay-Signature"].FirstOrDefault()
                     ?? Request.Headers["X-Hub-Signature-256"].FirstOrDefault()
                     ?? "";

        if (!string.IsNullOrEmpty(signature) && !_shiprelayService.ValidateWebhookSignature(rawBody, signature))
        {
            await _eventLog.LogWarning("ShiprelayWebhook", "INVALID_SIGNATURE",
                "Webhook signature validation failed");
            return Unauthorized();
        }

        ShiprelayWebhookPayload? payload;
        try
        {
            payload = JsonSerializer.Deserialize<ShiprelayWebhookPayload>(rawBody,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch
        {
            return BadRequest("Invalid payload");
        }

        if (payload == null) return BadRequest("Empty payload");

        await HandleWebhookEventAsync(payload);
        return Ok(new { received = true });
    }

    private async Task HandleWebhookEventAsync(ShiprelayWebhookPayload payload)
    {
        // Match order by ShiprelayShipmentId or order_ref — direct DB access to avoid
        // triggering ShipRelay calls that live inside OrderService
        var order = await _ctx.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o =>
                o.ShiprelayShipmentId == payload.ShipmentId
                || o.OrderCode == payload.OrderRef);

        if (order == null)
        {
            await _eventLog.LogWarning("ShiprelayWebhook", "ORDER_NOT_FOUND",
                $"No order found for ShipmentId={payload.ShipmentId}, OrderRef={payload.OrderRef}");
            return; // Return 200 so ShipRelay does not retry
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

        // Also update ShiprelayShipmentId if we matched by order_ref and didn't have it yet
        if (string.IsNullOrEmpty(order.ShiprelayShipmentId) && !string.IsNullOrEmpty(payload.ShipmentId))
            order.ShiprelayShipmentId = payload.ShipmentId;

        _ctx.OrderHistories.Add(new OrderHistory
        {
            OrderId        = order.ItemID,
            FromStatus     = prevStatus,
            ToStatus       = newStatus.Value,
            Comment        = $"ShipRelay: {payload.Status}",
            IsSystemAction = true
        });

        if (newStatus == OrderStatus.Confirmed && payload.Status?.ToLower() == "held")
            await _eventLog.LogWarning("ShiprelayWebhook", "SHIPMENT_HELD",
                $"ShipRelay held shipment for OrderId={order.ItemID}, ShipmentId={payload.ShipmentId}");

        await _ctx.SaveChangesAsync();

        await _eventLog.LogInformation("ShiprelayWebhook", "WEBHOOK_PROCESSED",
            $"Order {order.OrderCode}: {prevStatus} → {newStatus}, ShipmentId={payload.ShipmentId}");
    }
}
