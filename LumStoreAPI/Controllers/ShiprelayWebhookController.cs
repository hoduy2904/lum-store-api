using System.Text;
using System.Text.Json;
using LumStoreAPI.Application.DTOs.ShiprelayDTO;
using LumStoreAPI.Application.Interfaces;
using LumStoreAPI.Core.Interfaces.Sytems;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LumStoreAPI.Controllers;

/// <summary>Receives inbound webhook events from Shiprelay.</summary>
[Route("api/webhooks/shiprelay")]
[ApiController]
[ApiExplorerSettings(GroupName = "Webhook")]
[AllowAnonymous]
public class ShiprelayWebhookController(
    IShiprelayService shiprelayService,
    IShiprelayWebhookService webhookService,
    IEventLogService eventLog) : ControllerBase
{
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

        if (!string.IsNullOrEmpty(signature) && !shiprelayService.ValidateWebhookSignature(rawBody, signature))
        {
            await eventLog.LogWarning("ShiprelayWebhook", "INVALID_SIGNATURE",
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

        await webhookService.HandleEventAsync(payload);
        return Ok(new { received = true });
    }
}
