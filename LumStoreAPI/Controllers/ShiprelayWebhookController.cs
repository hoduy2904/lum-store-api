using System.Text;
using System.Text.Json;
using LumStoreAPI.Application.DTOs.OrderDTO;
using LumStoreAPI.Application.DTOs.ShiprelayDTO;
using LumStoreAPI.Application.Interfaces;
using LumStoreAPI.Core.Interfaces.Sytems;
using LumStoreAPI.Core.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LumStoreAPI.Controllers;

/// <summary>Receives inbound webhook events from Shiprelay.</summary>
[Route("api/webhooks/shiprelay")]
[ApiController]
[AllowAnonymous]
public class ShiprelayWebhookController : ControllerBase
{
    private readonly IShiprelayService _shiprelayService;
    private readonly IOrderService _orderService;
    private readonly IEventLogService _eventLog;

    public ShiprelayWebhookController(
        IShiprelayService shiprelayService,
        IOrderService orderService,
        IEventLogService eventLog)
    {
        _shiprelayService = shiprelayService;
        _orderService = orderService;
        _eventLog = eventLog;
    }

    /// <summary>POST /api/webhooks/shiprelay — Receive Shiprelay events.</summary>
    [HttpPost]
    public async Task<IActionResult> Receive()
    {
        // Read raw body for signature validation
        Request.EnableBuffering();
        using var reader = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true);
        var rawBody = await reader.ReadToEndAsync();
        Request.Body.Position = 0;

        // Validate signature
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
        // Find order by Shiprelay shipment ID or order reference
        var orderCode = payload.OrderReference;
        if (string.IsNullOrEmpty(orderCode)) return;

        // Strip "ORD-" prefix if present (we set order_reference as "ORD-{orderId}")
        // or look by orderCode directly
        var order = await _orderService.GetOrderByCodeAsync(orderCode);
        if (order == null) return;

        switch (payload.Event?.ToLower())
        {
            case "shipment.created":
            case "shipment.label_printed":
                await _orderService.UpdateOrderStatusAsync(order.OrderId, new OrderUpdateStatusDTO
                {
                    NewStatus = OrderStatus.Shipped,
                    Comment = $"Shiprelay: {payload.Event}. Tracking: {payload.TrackingNumber}"
                });
                break;

            case "shipment.in_transit":
                // Update tracking info without changing status
                break;

            case "shipment.delivered":
                await _orderService.UpdateOrderStatusAsync(order.OrderId, new OrderUpdateStatusDTO
                {
                    NewStatus = OrderStatus.Delivered,
                    Comment = $"Shiprelay: Delivered at {payload.DeliveredAt:u}"
                });
                break;

            case "shipment.cancelled":
                await _orderService.UpdateOrderStatusAsync(order.OrderId, new OrderUpdateStatusDTO
                {
                    NewStatus = OrderStatus.Cancelled,
                    Comment = "Shiprelay: Shipment cancelled"
                });
                break;
        }

        await _eventLog.LogInformation("ShiprelayWebhook", "WEBHOOK_RECEIVED",
            $"Event={payload.Event}, ShipmentId={payload.ShipmentId}, OrderRef={payload.OrderReference}");
    }
}
