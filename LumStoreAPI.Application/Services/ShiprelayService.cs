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

    // ── Shipments ─────────────────────────────────────────────────────────

    public async Task<ShiprelayShipmentResult> CreateShipmentAsync(ShiprelayCreateShipmentDTO dto)
    {
        var (client, resellerId) = await BuildClientAsync();

        var payload = new
        {
            reseller_id = resellerId,
            order_ref = dto.OrderRef,
            shipment_total_cost = dto.ShipmentTotalCost,
            package_ref = dto.PackageRef,
            shipment_created_at = dto.ShipmentCreatedAt.ToString("o"),
            type = dto.Type ?? "b2c",
            notes = dto.Notes,
            tags = dto.Tags,
            shipping_selected_ref = dto.ShippingSelectedRef,
            address = new
            {
                name = dto.RecipientName,
                company = dto.Company,
                address1 = dto.Address1,
                address2 = dto.Address2,
                city = dto.City,
                region = dto.State,
                country = dto.Country,
                zip = dto.Zip,
                phone = dto.Phone,
                email = dto.Email
            },
            items = dto.Items.Select(i => new
            {
                product_id = i.ProductId,
                quantity = i.Quantity,
                price = i.Price,
                currency = i.Currency ?? "USD"
            })
        };

        try
        {
            var response = await client.PostAsJsonAsync("shipments", payload);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                await _eventLog.LogWarning("ShiprelayService", "SHIPRELAY_CREATE_FAILED",
                    $"POST shipments failed | OrderRef={dto.OrderRef} | HTTP {(int)response.StatusCode} | {content}");
                return new ShiprelayShipmentResult { Success = false, ErrorMessage = content };
            }

            var result = JsonSerializer.Deserialize<JsonElement>(content);
            var shipmentResult = new ShiprelayShipmentResult
            {
                Success = true,
                ShipmentId = GetString(result, "id") ?? GetString(result, "shipment_id") ?? "",
                TrackingNumber = GetString(result, "tracking_number"),
                TrackingUrl = GetString(result, "tracking_url"),
                Carrier = ExtractCarrierName(result),
                Status = GetString(result, "status") ?? "queued"
            };

            await _eventLog.LogInformation("ShiprelayService", "SHIPRELAY_CREATE_SUCCESS",
                $"POST shipments OK | OrderRef={dto.OrderRef} | ShipmentId={shipmentResult.ShipmentId} | Status={shipmentResult.Status}");

            return shipmentResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Shiprelay CreateShipment error for OrderRef={OrderRef}", dto.OrderRef);
            await _eventLog.LogWarning("ShiprelayService", "SHIPRELAY_CREATE_EXCEPTION",
                $"POST shipments exception | OrderRef={dto.OrderRef} | {ex.Message}");
            return new ShiprelayShipmentResult { Success = false, ErrorMessage = ex.Message };
        }
    }

    public async Task<ShiprelayShipmentResult> UpdateShipmentAsync(string shipmentId, ShiprelayUpdateShipmentDTO dto)
    {
        var (client, _) = await BuildClientAsync();

        var payload = new
        {
            shipment_total_cost = dto.ShipmentTotalCost,
            order_ref = dto.OrderRef,
            type = dto.Type,
            notes = dto.Notes,
            tags = dto.Tags,
            shipping_selected_ref = dto.ShippingSelectedRef,
            items = dto.Items.Select(i => new
            {
                product_id = i.ProductId,
                quantity = i.Quantity,
                price = i.Price,
                currency = i.Currency ?? "USD"
            })
        };

        try
        {
            var response = await client.PutAsJsonAsync($"shipments/{shipmentId}", payload);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                await _eventLog.LogWarning("ShiprelayService", "SHIPRELAY_UPDATE_FAILED",
                    $"PUT shipments/{shipmentId} failed | HTTP {(int)response.StatusCode} | {content}");
                return new ShiprelayShipmentResult { Success = false, ErrorMessage = content };
            }

            var result = JsonSerializer.Deserialize<JsonElement>(content);
            return new ShiprelayShipmentResult
            {
                Success = true,
                ShipmentId = shipmentId,
                TrackingNumber = GetString(result, "tracking_number"),
                TrackingUrl = GetString(result, "tracking_url"),
                Carrier = ExtractCarrierName(result),
                Status = GetString(result, "status") ?? ""
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Shiprelay UpdateShipment error for ShipmentId={ShipmentId}", shipmentId);
            await _eventLog.LogWarning("ShiprelayService", "SHIPRELAY_UPDATE_EXCEPTION",
                $"PUT shipments/{shipmentId} exception | {ex.Message}");
            return new ShiprelayShipmentResult { Success = false, ErrorMessage = ex.Message };
        }
    }

    public async Task<ShiprelayTrackingResult?> GetTrackingAsync(string shipmentId)
    {
        var (client, _) = await BuildClientAsync();
        try
        {
            var response = await client.GetAsync($"shipments/{shipmentId}");
            if (!response.IsSuccessStatusCode)
            {
                await _eventLog.LogWarning("ShiprelayService", "SHIPRELAY_TRACKING_FAILED",
                    $"GET shipments/{shipmentId} | HTTP {(int)response.StatusCode}");
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(content);

            return new ShiprelayTrackingResult
            {
                ShipmentId = shipmentId,
                TrackingNumber = GetString(result, "tracking_number"),
                TrackingUrl = GetString(result, "tracking_url"),
                Carrier = ExtractCarrierName(result),
                Status = GetString(result, "status") ?? "unknown"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Shiprelay GetTracking error for ShipmentId={ShipmentId}", shipmentId);
            await _eventLog.LogWarning("ShiprelayService", "SHIPRELAY_TRACKING_EXCEPTION",
                $"GET shipments/{shipmentId} exception | {ex.Message}");
            return null;
        }
    }

    public async Task<IEnumerable<ShiprelayShipmentSummaryDTO>> GetShipmentsAsync(ShiprelayGetShipmentsRequest request)
    {
        var (client, _) = await BuildClientAsync();
        try
        {
            var qs = BuildShipmentsQueryString(request);
            var response = await client.GetAsync($"shipments{qs}");
            if (!response.IsSuccessStatusCode)
            {
                await _eventLog.LogWarning("ShiprelayService", "SHIPRELAY_LIST_FAILED",
                    $"GET shipments failed | HTTP {(int)response.StatusCode}");
                return [];
            }

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(content);

            if (!result.TryGetProperty("data", out var data) || data.ValueKind != JsonValueKind.Array)
                return [];

            return data.EnumerateArray().Select(s => new ShiprelayShipmentSummaryDTO
            {
                Id = GetString(s, "id") ?? "",
                Status = GetString(s, "status"),
                OrderRef = GetString(s, "order_ref"),
                SourceOrderId = GetString(s, "source_order_id"),
                TrackingNumber = GetString(s, "tracking_number"),
                TrackingUrl = GetString(s, "tracking_url"),
                Carrier = ExtractCarrierName(s),
                UpdatedAt = s.TryGetProperty("updated_at", out var ua) && ua.TryGetDateTime(out var dt) ? dt : null
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Shiprelay GetShipments error");
            await _eventLog.LogWarning("ShiprelayService", "SHIPRELAY_LIST_EXCEPTION",
                $"GET shipments exception | {ex.Message}");
            return [];
        }
    }

    public async Task<bool> CancelShipmentAsync(string shipmentId)
    {
        var (client, _) = await BuildClientAsync();
        try
        {
            var response = await client.PatchAsync($"shipments/{shipmentId}/archive", null);
            if (response.IsSuccessStatusCode)
            {
                await _eventLog.LogInformation("ShiprelayService", "SHIPRELAY_CANCEL_SUCCESS",
                    $"PATCH shipments/{shipmentId}/archive OK");
                return true;
            }

            var err = await response.Content.ReadAsStringAsync();
            await _eventLog.LogWarning("ShiprelayService", "SHIPRELAY_CANCEL_FAILED",
                $"PATCH shipments/{shipmentId}/archive | HTTP {(int)response.StatusCode} | {err}");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Shiprelay CancelShipment error for ShipmentId={ShipmentId}", shipmentId);
            await _eventLog.LogWarning("ShiprelayService", "SHIPRELAY_CANCEL_EXCEPTION",
                $"PATCH shipments/{shipmentId}/archive exception | {ex.Message}");
            return false;
        }
    }

    public async Task<bool> RestoreShipmentAsync(string shipmentId)
    {
        var (client, _) = await BuildClientAsync();
        try
        {
            var response = await client.PatchAsync($"shipments/{shipmentId}/restore", null);
            if (response.IsSuccessStatusCode)
            {
                await _eventLog.LogInformation("ShiprelayService", "SHIPRELAY_RESTORE_SUCCESS",
                    $"PATCH shipments/{shipmentId}/restore OK");
                return true;
            }

            var err = await response.Content.ReadAsStringAsync();
            await _eventLog.LogWarning("ShiprelayService", "SHIPRELAY_RESTORE_FAILED",
                $"PATCH shipments/{shipmentId}/restore | HTTP {(int)response.StatusCode} | {err}");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Shiprelay RestoreShipment error for ShipmentId={ShipmentId}", shipmentId);
            await _eventLog.LogWarning("ShiprelayService", "SHIPRELAY_RESTORE_EXCEPTION",
                $"PATCH shipments/{shipmentId}/restore exception | {ex.Message}");
            return false;
        }
    }

    // ── Rates ─────────────────────────────────────────────────────────────

    public async Task<IEnumerable<ShiprelayRateResult>> GetRatesAsync(ShiprelayRateRequestDTO dto)
    {
        var (client, resellerId) = await BuildClientAsync();

        if (!string.IsNullOrEmpty(dto.SessionId))
            client.DefaultRequestHeaders.TryAddWithoutValidation("X-Rate-Session", dto.SessionId);

        var payload = new
        {
            reseller_id = resellerId,
            destination = new
            {
                name = dto.RecipientName,
                company = dto.Company,
                address1 = dto.Address1,
                address2 = dto.Address2,
                city = dto.City,
                region = dto.Region,
                country = dto.Country,
                zip = dto.Zip,
                phone = dto.Phone,
                email = dto.Email
            },
            items = dto.Items.Select(i => new
            {
                product_id = i.ProductId,
                quantity = i.Quantity,
                price = i.Price,
                currency = i.Currency ?? "USD"
            })
        };

        try
        {
            var response = await client.PostAsJsonAsync("rates/calculate", payload);
            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync();
                await _eventLog.LogWarning("ShiprelayService", "SHIPRELAY_RATES_FAILED",
                    $"POST rates/calculate failed | OrderId={dto.OrderId} | HTTP {(int)response.StatusCode} | {err}");
                return [];
            }

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(content);

            if (!result.TryGetProperty("data", out var dataEl) || dataEl.ValueKind != JsonValueKind.Array)
                return [];

            var rates = dataEl.EnumerateArray().Select(r => new ShiprelayRateResult
            {
                ServiceCode = GetString(r, "service_code") ?? "",
                ServiceName = GetString(r, "service_name") ?? "",
                TotalPrice = r.TryGetProperty("total_price", out var tp) ? tp.GetDecimal() : 0,
                Description = GetString(r, "description"),
                Currency = GetString(r, "currency") ?? "USD",
                MinDeliveryDate = r.TryGetProperty("min_delivery_date", out var minD) && minD.TryGetDateTime(out var min) ? min : null,
                MaxDeliveryDate = r.TryGetProperty("max_delivery_date", out var maxD) && maxD.TryGetDateTime(out var max) ? max : null,
                PhoneRequired = r.TryGetProperty("phone_required", out var pr) && pr.GetBoolean()
            }).ToList();

            await _eventLog.LogInformation("ShiprelayService", "SHIPRELAY_RATES_SUCCESS",
                $"POST rates/calculate OK | OrderId={dto.OrderId} | {rates.Count} rates returned");

            return rates;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Shiprelay GetRates error for OrderId={OrderId}", dto.OrderId);
            await _eventLog.LogWarning("ShiprelayService", "SHIPRELAY_RATES_EXCEPTION",
                $"POST rates/calculate exception | OrderId={dto.OrderId} | {ex.Message}");
            return [];
        }
    }

    // ── Products ──────────────────────────────────────────────────────────

    public async Task<ShiprelayProductDTO?> GetProductBySkuAsync(string sku)
    {
        try
        {
            var (client, _) = await BuildClientAsync();
            var response = await client.GetAsync($"products?sku={Uri.EscapeDataString(sku)}&per_page=1");
            if (!response.IsSuccessStatusCode) return null;

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(content);

            if (!result.TryGetProperty("data", out var data) || data.GetArrayLength() == 0)
                return null;

            var item = data[0];
            return new ShiprelayProductDTO
            {
                Sku = GetString(item, "sku") ?? sku,
                AvailableStock = item.TryGetProperty("available_count", out var qty) ? qty.GetInt32()
                    : item.TryGetProperty("stock_count", out var sc) ? sc.GetInt32() : 0
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Shiprelay GetProductBySku error for SKU={Sku}", sku);
            await _eventLog.LogWarning("ShiprelayService", "SHIPRELAY_GET_PRODUCT_EXCEPTION",
                $"GET products?sku={sku} exception | {ex.Message}");
            return null;
        }
    }

    // ── Webhook ───────────────────────────────────────────────────────────

    public bool ValidateWebhookSignature(string payload, string signature)
    {
        var config = _configRepo.GetConfigByTypeAsync(IntegrationType.Shiprelay).GetAwaiter().GetResult();
        if (config?.WebhookSecret == null) return false;

        var key = Encoding.UTF8.GetBytes(config.WebhookSecret);
        using var hmac = new HMACSHA256(key);
        var computed = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        var computedHex = "sha256=" + Convert.ToHexString(computed).ToLower();
        return string.Equals(computedHex, signature, StringComparison.OrdinalIgnoreCase);
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    /// <summary>
    /// Builds the HTTP client and returns the reseller_id from config.ApiSecret.
    /// ApiKey = Bearer token, ApiSecret = reseller_id (UUID provided by ShipRelay).
    /// </summary>
    private async Task<(HttpClient Client, string ResellerId)> BuildClientAsync()
    {
        var config = await _configRepo.GetConfigByTypeAsync(IntegrationType.Shiprelay)
            ?? throw new InvalidOperationException("Shiprelay integration is not configured or disabled");

        var client = _httpClientFactory.CreateClient("Shiprelay");
        client.BaseAddress = new Uri(config.BaseUrl.TrimEnd('/') + "/");
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {config.ApiKey}");
        client.DefaultRequestHeaders.Add("Accept", "application/json");

        return (client, config.ApiSecret ?? string.Empty);
    }

    private static string? GetString(JsonElement el, string key)
        => el.TryGetProperty(key, out var prop) && prop.ValueKind == JsonValueKind.String
            ? prop.GetString()
            : null;

    /// <summary>Extracts carrier name from carrier field which may be a string or an object with a "name" property.</summary>
    private static string? ExtractCarrierName(JsonElement el)
    {
        if (!el.TryGetProperty("carrier", out var carrier)) return null;
        if (carrier.ValueKind == JsonValueKind.String) return carrier.GetString();
        if (carrier.ValueKind == JsonValueKind.Object) return GetString(carrier, "name");
        return null;
    }

    private static string BuildShipmentsQueryString(ShiprelayGetShipmentsRequest r)
    {
        var parts = new List<string>
        {
            $"page={r.Page}",
            $"per_page={r.PerPage}"
        };
        if (!string.IsNullOrEmpty(r.SourceOrderId)) parts.Add($"source_order_id={Uri.EscapeDataString(r.SourceOrderId)}");
        if (!string.IsNullOrEmpty(r.OrderRef)) parts.Add($"order_ref={Uri.EscapeDataString(r.OrderRef)}");
        if (!string.IsNullOrEmpty(r.Status)) parts.Add($"status={Uri.EscapeDataString(r.Status)}");
        if (!string.IsNullOrEmpty(r.TrackingNumber)) parts.Add($"tracking_number={Uri.EscapeDataString(r.TrackingNumber)}");
        if (!string.IsNullOrEmpty(r.UpdatedAtFrom)) parts.Add($"updated_at_from={Uri.EscapeDataString(r.UpdatedAtFrom)}");
        if (!string.IsNullOrEmpty(r.UpdatedAtTo)) parts.Add($"updated_at_to={Uri.EscapeDataString(r.UpdatedAtTo)}");
        return "?" + string.Join("&", parts);
    }
}
