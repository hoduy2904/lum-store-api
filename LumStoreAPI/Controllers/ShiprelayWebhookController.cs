using System.Text.Json;
using LumStoreAPI.Application.DTOs.ShiprelayDTO;
using LumStoreAPI.Application.Interfaces;
using LumStoreAPI.Core.Entities.Integrations;
using LumStoreAPI.Core.Interfaces.Repositories;
using LumStoreAPI.Core.Interfaces.Sytems;
using LumStoreAPI.Core.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LumStoreAPI.Controllers;

/// <summary>Receives inbound webhook events from Shiprelay.</summary>
[Route("api/integrations/webhook/shiprelay")]
[Route("api/webhooks/shiprelay")]
[ApiController]
[AllowAnonymous]
public class ShiprelayWebhookController(
    IShiprelayService shiprelayService,
    IShiprelayWebhookService webhookService,
    IEventLogService eventLog,
    IIntegrationConfigRepository integrationConfigRepository) : ControllerBase
{
    private static readonly JsonSerializerOptions WebhookJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static readonly HashSet<string> SupportedStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "queued",
        "held",
        "requested",
        "processing",
        "shipped",
        "returned",
        "inactive",
        "in_transit",
        "delivered"
    };

    /// <summary>POST /api/webhooks/shiprelay — Receive Shiprelay events.</summary>
    [HttpPost]
    [Consumes("application/json")]
    public async Task<IActionResult> Receive()
    {
        Request.EnableBuffering();
        using var rawBodyStream = new MemoryStream();
        await Request.Body.CopyToAsync(rawBodyStream);
        var rawBody = rawBodyStream.ToArray();
        Request.Body.Position = 0;

        var signature = GetSignatureHeader();

        if (string.IsNullOrWhiteSpace(signature))
        {
            await eventLog.LogWarning("ShiprelayWebhook", "MISSING_SIGNATURE",
                "Webhook signature header is missing");
            await WriteWebhookSyncLogAsync(SyncStatus.Failed,
                "Shiprelay webhook rejected: missing signature",
                "Header 'signature' is required");
            return Unauthorized();
        }

        if (!await shiprelayService.ValidateWebhookSignatureAsync(rawBody, signature))
        {
            await eventLog.LogWarning("ShiprelayWebhook", "INVALID_SIGNATURE",
                "Webhook signature validation failed");
            await WriteWebhookSyncLogAsync(SyncStatus.Failed,
                "Shiprelay webhook rejected: invalid signature",
                "HMAC-SHA256 signature did not match the raw request body");
            return Unauthorized();
        }

        ShiprelayWebhookPayload? payload;
        try
        {
            payload = JsonSerializer.Deserialize<ShiprelayWebhookPayload>(rawBody, WebhookJsonOptions);
        }
        catch (JsonException)
        {
            await WriteWebhookSyncLogAsync(SyncStatus.Failed,
                "Shiprelay webhook rejected: invalid JSON",
                "Request body could not be parsed as Shiprelay webhook JSON");
            return BadRequest("Invalid payload");
        }

        if (payload == null)
        {
            await WriteWebhookSyncLogAsync(SyncStatus.Failed,
                "Shiprelay webhook rejected: empty payload");
            return BadRequest("Empty payload");
        }

        var validationErrors = ValidatePayload(payload).ToArray();
        if (validationErrors.Length > 0)
        {
            await WriteWebhookSyncLogAsync(SyncStatus.Failed,
                "Shiprelay webhook rejected: invalid payload",
                string.Join("; ", validationErrors));
            return BadRequest(new { errors = validationErrors });
        }

        await webhookService.HandleEventAsync(payload);
        return Ok(new { received = true });
    }

    private string? GetSignatureHeader()
        => Request.Headers["signature"].FirstOrDefault()
        ?? Request.Headers["X-Shiprelay-Signature"].FirstOrDefault()
        ?? Request.Headers["X-Hub-Signature-256"].FirstOrDefault();

    private static IEnumerable<string> ValidatePayload(ShiprelayWebhookPayload payload)
    {
        if (string.IsNullOrWhiteSpace(payload.OrderRef))
            yield return "order_ref is required";

        if (string.IsNullOrWhiteSpace(payload.Status))
        {
            yield return "status is required";
        }
        else if (!SupportedStatuses.Contains(payload.Status))
        {
            yield return $"status '{payload.Status}' is not supported";
        }
    }

    private Task WriteWebhookSyncLogAsync(SyncStatus status, string summary, string? errorDetail = null)
    {
        var now = DateTimeOffset.UtcNow;
        return integrationConfigRepository.InsertSyncLogAsync(new SyncLog
        {
            IntegrationType = IntegrationType.Shiprelay,
            SyncMode = SyncMode.Webhook,
            Status = status,
            Summary = summary,
            ErrorDetail = errorDetail,
            RecordsSynced = status == SyncStatus.Success ? 1 : 0,
            RecordsFailed = status == SyncStatus.Success ? 0 : 1,
            StartedAt = now,
            CompletedAt = now
        });
    }
}
