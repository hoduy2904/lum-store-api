using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using LumStoreAPI.Application.DTOs.ShiprelayDTO;
using LumStoreAPI.Application.Interfaces;
using LumStoreAPI.Core.Entities.Integrations;
using LumStoreAPI.Core.Interfaces.Sytems;
using LumStoreAPI.Core.Models.Enums;
using LumStoreAPI.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace LumStoreAPI.Application.Services;

public class ShiprelayService : IShiprelayService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IIntegrationConfigRepository _configRepo;
    private readonly IEventLogService _eventLog;
    private readonly ILogger<ShiprelayService> _logger;

    public ShiprelayService(
        IHttpClientFactory httpClientFactory,
        IIntegrationConfigRepository configRepo,
        IEventLogService eventLog,
        ILogger<ShiprelayService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configRepo = configRepo;
        _eventLog = eventLog;
        _logger = logger;
    }

    public async Task<ShiprelayShipmentResult> CreateShipmentAsync(ShiprelayCreateShipmentDTO dto)
    {
        var client = await BuildClientAsync();

        var payload = new
        {
            recipient = new
            {
                name = dto.RecipientName,
                address1 = dto.Address1,
                address2 = dto.Address2,
                city = dto.City,
                state = dto.State,
                zip = dto.Zip,
                country = dto.Country,
                phone = dto.Phone,
                email = dto.Email
            },
            items = dto.Items.Select(i => new { sku = i.Sku, qty = i.Quantity, name = i.ProductName }),
            service_code = dto.ServiceCode,
            warehouse_id = dto.WarehouseId,
            notes = dto.Notes,
            order_reference = $"ORD-{dto.OrderId}"
        };

        try
        {
            var response = await client.PostAsJsonAsync("shipments", payload);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                await _eventLog.LogWarning("ShiprelayService", "SHIPRELAY_CREATE_FAILED",
                    $"Create shipment failed for OrderId={dto.OrderId}: {content}");
                return new ShiprelayShipmentResult { Success = false, ErrorMessage = content };
            }

            var result = JsonSerializer.Deserialize<JsonElement>(content);
            return new ShiprelayShipmentResult
            {
                Success = true,
                ShipmentId = GetString(result, "id") ?? GetString(result, "shipment_id") ?? "",
                TrackingNumber = GetString(result, "tracking_number"),
                TrackingUrl = GetString(result, "tracking_url"),
                Carrier = GetString(result, "carrier"),
                Status = GetString(result, "status") ?? "created"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Shiprelay CreateShipment error for OrderId={OrderId}", dto.OrderId);
            return new ShiprelayShipmentResult { Success = false, ErrorMessage = ex.Message };
        }
    }

    public async Task<ShiprelayTrackingResult?> GetTrackingAsync(string shipmentId)
    {
        var client = await BuildClientAsync();
        try
        {
            var response = await client.GetAsync($"shipments/{shipmentId}");
            if (!response.IsSuccessStatusCode) return null;

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(content);

            var events = new List<TrackingEventDTO>();
            if (result.TryGetProperty("events", out var eventsEl) && eventsEl.ValueKind == JsonValueKind.Array)
            {
                foreach (var ev in eventsEl.EnumerateArray())
                {
                    events.Add(new TrackingEventDTO
                    {
                        Timestamp = ev.TryGetProperty("timestamp", out var ts) && ts.TryGetDateTimeOffset(out var dt) ? dt : DateTimeOffset.UtcNow,
                        Description = GetString(ev, "description") ?? "",
                        Location = GetString(ev, "location")
                    });
                }
            }

            return new ShiprelayTrackingResult
            {
                ShipmentId = shipmentId,
                TrackingNumber = GetString(result, "tracking_number"),
                TrackingUrl = GetString(result, "tracking_url"),
                Carrier = GetString(result, "carrier"),
                Status = GetString(result, "status") ?? "unknown",
                StatusDescription = GetString(result, "status_description"),
                Events = events
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Shiprelay GetTracking error for ShipmentId={ShipmentId}", shipmentId);
            return null;
        }
    }

    public async Task<IEnumerable<ShiprelayRateResult>> GetRatesAsync(ShiprelayRateRequestDTO dto)
    {
        var client = await BuildClientAsync();
        var payload = new
        {
            to_zip = dto.ToZip,
            to_country = dto.ToCountry,
            items = dto.Items.Select(i => new { sku = i.Sku, qty = i.Quantity }),
            order_reference = $"ORD-{dto.OrderId}"
        };

        try
        {
            var response = await client.PostAsJsonAsync("rates", payload);
            if (!response.IsSuccessStatusCode) return [];

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(content);

            if (!result.TryGetProperty("rates", out var ratesEl) || ratesEl.ValueKind != JsonValueKind.Array)
                return [];

            return ratesEl.EnumerateArray().Select(r => new ShiprelayRateResult
            {
                ServiceCode = GetString(r, "service_code") ?? "",
                ServiceName = GetString(r, "service_name") ?? "",
                Carrier = GetString(r, "carrier") ?? "",
                Price = r.TryGetProperty("price", out var price) ? price.GetDecimal() : 0,
                Currency = GetString(r, "currency") ?? "USD",
                EstimatedDays = r.TryGetProperty("estimated_days", out var days) ? days.GetInt32() : null
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Shiprelay GetRates error for OrderId={OrderId}", dto.OrderId);
            return [];
        }
    }

    public async Task<bool> CancelShipmentAsync(string shipmentId)
    {
        var client = await BuildClientAsync();
        try
        {
            var response = await client.DeleteAsync($"shipments/{shipmentId}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Shiprelay CancelShipment error for ShipmentId={ShipmentId}", shipmentId);
            return false;
        }
    }

    public bool ValidateWebhookSignature(string payload, string signature)
    {
        // Shiprelay uses HMAC-SHA256 signature validation
        // signature is typically "sha256=<hex_digest>"
        var config = _configRepo.GetConfigByTypeAsync(IntegrationType.Shiprelay).GetAwaiter().GetResult();
        if (config?.WebhookSecret == null) return false;

        var key = Encoding.UTF8.GetBytes(config.WebhookSecret);
        using var hmac = new HMACSHA256(key);
        var computed = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        var computedHex = "sha256=" + Convert.ToHexString(computed).ToLower();
        return string.Equals(computedHex, signature, StringComparison.OrdinalIgnoreCase);
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private async Task<HttpClient> BuildClientAsync()
    {
        var config = await _configRepo.GetConfigByTypeAsync(IntegrationType.Shiprelay)
            ?? throw new InvalidOperationException("Shiprelay integration is not configured or disabled");

        var client = _httpClientFactory.CreateClient("Shiprelay");
        client.BaseAddress = new Uri(config.BaseUrl.TrimEnd('/') + "/");
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {config.ApiKey}");
        client.DefaultRequestHeaders.Add("Accept", "application/json");
        return client;
    }

    private static string? GetString(JsonElement el, string key)
        => el.TryGetProperty(key, out var prop) && prop.ValueKind == JsonValueKind.String
            ? prop.GetString()
            : null;
}
