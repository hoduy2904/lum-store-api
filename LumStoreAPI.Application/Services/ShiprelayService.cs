using LumStoreAPI.Application.DTOs.ShiprelayDTO;
using LumStoreAPI.Application.Exceptions;
using LumStoreAPI.Application.Interfaces;
using LumStoreAPI.Core.Interfaces.Repositories;
using LumStoreAPI.Core.Interfaces.Sytems;
using LumStoreAPI.Core.Models.Enums;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace LumStoreAPI.Application.Services;

public class ShiprelayService : IShiprelayService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IIntegrationConfigRepository _configRepo;
    private readonly IEventLogService _eventLog;
    private readonly ILogger<ShiprelayService> _logger;
    private readonly IMemoryCache _cache;

    // ShipRelay rate sessions are valid for 5 minutes. We cache the UUID and reuse it
    // via the X-Rate-Session header to avoid creating a new session on every call.
    private const string RateSessionCacheKey = "shiprelay_rate_session";

    public ShiprelayService(
        IHttpClientFactory httpClientFactory,
        IIntegrationConfigRepository configRepo,
        IEventLogService eventLog,
        ILogger<ShiprelayService> logger,
        IMemoryCache cache)
    {
        _httpClientFactory = httpClientFactory;
        _configRepo = configRepo;
        _eventLog = eventLog;
        _logger = logger;
        _cache = cache;
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
                    "POST shipments failed",
                    $"OrderRef={dto.OrderRef} | HTTP {(int)response.StatusCode} | {content}");
                return new ShiprelayShipmentResult { Success = false, ErrorMessage = content };
            }

            JsonElement result;
            try
            {
                result = JsonSerializer.Deserialize<JsonElement>(content);
            }
            catch (JsonException jsonEx)
            {
                // ShipRelay accepted the shipment (HTTP 2xx) but returned unparseable JSON.
                // The shipment EXISTS remotely — aborting the order would lose it permanently.
                // Caller must catch ShiprelayIntegrationException.ShipmentMayExistOnRemote
                // and save a reconciliation record rather than aborting the order.
                _logger.LogError(jsonEx,
                    "ShipRelay returned HTTP {StatusCode} but unparseable JSON for OrderRef={OrderRef}. Raw={Raw}",
                    (int)response.StatusCode, dto.OrderRef, content);
                throw new ShiprelayIntegrationException(
                    "ShipRelay accepted shipment (HTTP 200) but returned unparseable response. " +
                    "Shipment may exist on ShipRelay side. Manual reconciliation required. Raw response logged.",
                    jsonEx,
                    shipmentMayExistOnRemote: true)
                {
                    RawResponse = content
                };
            }

            var shipmentResult = new ShiprelayShipmentResult
            {
                Success = true,
                ShipmentId = GetString(result, "id") ?? GetString(result, "shipment_id") ?? "",
                TrackingNumber = GetTrackingNumberFrom(result),
                TrackingUrl = BuildTrackingUrl(result),
                Carrier = ExtractCarrierName(result),
                Service = ExtractServiceName(result),
                Status = GetString(result, "status") ?? "queued"
            };

            await _eventLog.LogInformation("ShiprelayService", "SHIPRELAY_CREATE_SUCCESS",
                "POST shipments OK",
                $"OrderRef={dto.OrderRef} | ShipmentId={shipmentResult.ShipmentId} | Status={shipmentResult.Status}");

            return shipmentResult;
        }
        catch (Exception ex) when (ex is not ShiprelayIntegrationException)
        {
            // ShiprelayIntegrationException must propagate to caller for reconciliation handling.
            // All other exceptions are swallowed here and returned as a failure result.
            _logger.LogError(ex, "Shiprelay CreateShipment error for OrderRef={OrderRef}", dto.OrderRef);
            await _eventLog.LogWarning("ShiprelayService", "SHIPRELAY_CREATE_EXCEPTION",
                "POST shipments exception",
                $"OrderRef={dto.OrderRef} | {ex.Message}");
            return new ShiprelayShipmentResult { Success = false, ErrorMessage = ex.Message };
        }
    }

    public async Task<ShiprelayShipmentResult> UpdateShipmentAsync(string shipmentId, ShiprelayUpdateShipmentDTO dto)
    {
        // ShipRelay only allows PUT when status is queued or held; reject early with a clear message.
        var tracking = await GetTrackingAsync(shipmentId);
        if (tracking == null)
            return new ShiprelayShipmentResult
            {
                Success = false,
                ErrorMessage = $"Cannot update shipment '{shipmentId}' — status could not be retrieved from ShipRelay."
            };

        if (!tracking.Status.Equals("queued", StringComparison.OrdinalIgnoreCase)
            && !tracking.Status.Equals("held", StringComparison.OrdinalIgnoreCase))
            return new ShiprelayShipmentResult
            {
                Success = false,
                ErrorMessage = $"Cannot update shipment — current status is '{tracking.Status}'. Only queued or held shipments can be modified."
            };

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
                    "PUT shipments failed",
                    $"ShipmentId={shipmentId} | HTTP {(int)response.StatusCode} | {content}");
                return new ShiprelayShipmentResult { Success = false, ErrorMessage = content };
            }

            var result = JsonSerializer.Deserialize<JsonElement>(content);
            return new ShiprelayShipmentResult
            {
                Success = true,
                ShipmentId = shipmentId,
                TrackingNumber = GetTrackingNumberFrom(result),
                TrackingUrl = BuildTrackingUrl(result),
                Carrier = ExtractCarrierName(result),
                Service = ExtractServiceName(result),
                Status = GetString(result, "status") ?? ""
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Shiprelay UpdateShipment error for ShipmentId={ShipmentId}", shipmentId);
            await _eventLog.LogWarning("ShiprelayService", "SHIPRELAY_UPDATE_EXCEPTION",
                "PUT shipments exception",
                $"ShipmentId={shipmentId} | {ex.Message}");
            return new ShiprelayShipmentResult { Success = false, ErrorMessage = ex.Message };
        }
    }

    public async Task<ShiprelayTrackingResult?> GetTrackingAsync(string shipmentId)
    {
        var (client, _) = await BuildClientAsync();
        try
        {
            var response = await client.GetAsync($"shipments/{shipmentId}");
            var content = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                await _eventLog.LogWarning("ShiprelayService", "SHIPRELAY_TRACKING_FAILED",
                    "GET shipments failed",
                    $"ShipmentId={shipmentId} | HTTP {(int)response.StatusCode} | {content}");
                return null;
            }
            var root = JsonSerializer.Deserialize<JsonElement>(content);

            // ShipRelay API v2 wraps single-item GET responses in {"data": {...}}
            var result = root.TryGetProperty("data", out var data) && data.ValueKind == JsonValueKind.Object
                ? data
                : root;

            return new ShiprelayTrackingResult
            {
                ShipmentId = shipmentId,
                TrackingNumber = GetTrackingNumberFrom(result),
                TrackingUrl = BuildTrackingUrl(result),
                Carrier = ExtractCarrierName(result),
                Service = ExtractServiceName(result),
                Status = GetString(result, "status") ?? "unknown",
                StatusDescription = GetString(result, "status_description"),
                EstimatedDelivery = result.TryGetProperty("estimated_delivery_date", out var edd) && edd.TryGetDateTimeOffset(out var ed) ? ed : null,
                DeliveredAt = result.TryGetProperty("delivered_at", out var da) && da.TryGetDateTimeOffset(out var dat) ? dat : null
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Shiprelay GetTracking error for ShipmentId={ShipmentId}", shipmentId);
            await _eventLog.LogWarning("ShiprelayService", "SHIPRELAY_TRACKING_EXCEPTION",
                "GET shipments exception",
                $"ShipmentId={shipmentId} | {ex.Message}");
            return null;
        }
    }

    public async Task<ShiprelayPagedResult<ShiprelayShipmentSummaryDTO>> GetShipmentsAsync(ShiprelayGetShipmentsRequest request)
    {
        var (client, _) = await BuildClientAsync();
        try
        {
            var qs = BuildShipmentsQueryString(request);
            var response = await client.GetAsync($"shipments{qs}");
            if (!response.IsSuccessStatusCode)
            {
                await _eventLog.LogWarning("ShiprelayService", "SHIPRELAY_LIST_FAILED",
                    "GET shipments failed",
                    $"HTTP {(int)response.StatusCode}");
                return new ShiprelayPagedResult<ShiprelayShipmentSummaryDTO>();
            }

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(content);

            if (!result.TryGetProperty("data", out var data) || data.ValueKind != JsonValueKind.Array)
                return new ShiprelayPagedResult<ShiprelayShipmentSummaryDTO>();

            var items = data.EnumerateArray().Select(s => new ShiprelayShipmentSummaryDTO
            {
                Id = GetString(s, "id") ?? "",
                Status = GetString(s, "status"),
                OrderRef = GetString(s, "order_ref"),
                SourceOrderId = GetString(s, "source_order_id"),
                TrackingNumber = GetTrackingNumberFrom(s),
                TrackingUrl = BuildTrackingUrl(s),
                Carrier = ExtractCarrierName(s),
                UpdatedAt = s.TryGetProperty("updated_at", out var ua) && ua.TryGetDateTime(out var dt) ? dt : null
            }).ToList();

            var pagedResult = new ShiprelayPagedResult<ShiprelayShipmentSummaryDTO> { Data = items };
            if (result.TryGetProperty("meta", out var meta) && meta.ValueKind == JsonValueKind.Object)
            {
                pagedResult.Total       = meta.TryGetProperty("total",        out var tot)  ? tot.GetInt32()  : items.Count;
                pagedResult.CurrentPage = meta.TryGetProperty("current_page", out var cur)  ? cur.GetInt32()  : request.Page;
                pagedResult.LastPage    = meta.TryGetProperty("last_page",    out var last) ? last.GetInt32() : 1;
                pagedResult.PerPage     = meta.TryGetProperty("per_page",     out var pp)   ? pp.GetInt32()   : request.PerPage;
            }
            else
            {
                pagedResult.Total       = items.Count;
                pagedResult.CurrentPage = request.Page;
                pagedResult.LastPage    = 1;
                pagedResult.PerPage     = request.PerPage;
            }

            return pagedResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Shiprelay GetShipments error");
            await _eventLog.LogWarning("ShiprelayService", "SHIPRELAY_LIST_EXCEPTION",
                "GET shipments exception",
                $"{ex.Message}");
            return new ShiprelayPagedResult<ShiprelayShipmentSummaryDTO>();
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
                    "PATCH shipments/archive OK",
                    $"ShipmentId={shipmentId}");
                return true;
            }

            var err = await response.Content.ReadAsStringAsync();
            await _eventLog.LogWarning("ShiprelayService", "SHIPRELAY_CANCEL_FAILED",
                "PATCH shipments/archive failed",
                $"ShipmentId={shipmentId} | HTTP {(int)response.StatusCode} | {err}");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Shiprelay CancelShipment error for ShipmentId={ShipmentId}", shipmentId);
            await _eventLog.LogWarning("ShiprelayService", "SHIPRELAY_CANCEL_EXCEPTION",
                "PATCH shipments/archive exception",
                $"ShipmentId={shipmentId} | {ex.Message}");
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
                    "PATCH shipments/restore OK",
                    $"ShipmentId={shipmentId}");
                return true;
            }

            var err = await response.Content.ReadAsStringAsync();
            await _eventLog.LogWarning("ShiprelayService", "SHIPRELAY_RESTORE_FAILED",
                "PATCH shipments/restore failed",
                $"ShipmentId={shipmentId} | HTTP {(int)response.StatusCode} | {err}");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Shiprelay RestoreShipment error for ShipmentId={ShipmentId}", shipmentId);
            await _eventLog.LogWarning("ShiprelayService", "SHIPRELAY_RESTORE_EXCEPTION",
                "PATCH shipments/restore exception",
                $"ShipmentId={shipmentId} | {ex.Message}");
            return false;
        }
    }

    // ── Rates ─────────────────────────────────────────────────────────────

    public async Task<IEnumerable<ShiprelayRateResult>> GetRatesAsync(ShiprelayRateRequestDTO dto)
    {
        var (client, resellerId) = await BuildClientAsync();

        // Prefer a cached session UUID (reuses the 5-minute ShipRelay window).
        // Fall back to the caller-supplied SessionId, then omit the header entirely.
        var sessionToSend = _cache.TryGetValue(RateSessionCacheKey, out string? cached) && !string.IsNullOrEmpty(cached)
            ? cached
            : dto.SessionId;

        if (!string.IsNullOrEmpty(sessionToSend))
            client.DefaultRequestHeaders.TryAddWithoutValidation("X-Rate-Session", sessionToSend);

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
                    "POST rates/calculate failed",
                    $"OrderId={dto.OrderId} | HTTP {(int)response.StatusCode} | {err}");
                return [];
            }

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(content);

            if (!result.TryGetProperty("data", out var dataEl) || dataEl.ValueKind != JsonValueKind.Array)
                return [];

            var rates = dataEl.EnumerateArray().Select(r => new ShiprelayRateResult
            {
                CarrierId = GetString(r, "service_code") ?? "",
                CarrierName = GetString(r, "service_name") ?? "",
                Price = r.TryGetProperty("total_price", out var tp) ? tp.GetDecimal() : 0,
                Description = GetString(r, "description"),
                Currency = GetString(r, "currency") ?? "USD",
                MinDeliveryDate = r.TryGetProperty("min_delivery_date", out var minD) && minD.TryGetDateTime(out var min) ? min : null,
                MaxDeliveryDate = r.TryGetProperty("max_delivery_date", out var maxD) && maxD.TryGetDateTime(out var max) ? max : null,
                PhoneRequired = r.TryGetProperty("phone_required", out var pr) && pr.GetBoolean()
            }).ToList();

            // Cache the session UUID returned in meta so subsequent calls within
            // the same 5-minute window reuse it via X-Rate-Session, avoiding a new session.
            CacheRateSession(result);

            await _eventLog.LogInformation("ShiprelayService", "SHIPRELAY_RATES_SUCCESS",
                "POST rates/calculate OK",
                $"OrderId={dto.OrderId} | {rates.Count} rates returned | Session={(_cache.TryGetValue(RateSessionCacheKey, out string? s) ? s : "none")}");

            return rates;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Shiprelay GetRates error for OrderId={OrderId}", dto.OrderId);
            await _eventLog.LogWarning("ShiprelayService", "SHIPRELAY_RATES_EXCEPTION",
                "POST rates/calculate exception",
                $"OrderId={dto.OrderId} | {ex.Message}");
            return [];
        }
    }

    /// <summary>
    /// Extracts meta.session and meta.expired_at from the rate response and caches the session UUID.
    /// ShipRelay sessions are valid for 5 minutes; we cache with an absolute expiry matching expired_at.
    /// If expired_at is absent we conservatively default to 4 minutes.
    /// </summary>
    private void CacheRateSession(JsonElement responseRoot)
    {
        if (!responseRoot.TryGetProperty("meta", out var meta) || meta.ValueKind != JsonValueKind.Object)
            return;

        var session = GetString(meta, "session");
        if (string.IsNullOrEmpty(session)) return;

        DateTimeOffset expiry = meta.TryGetProperty("expired_at", out var ea)
            && ea.TryGetDateTimeOffset(out var parsedExpiry)
            ? parsedExpiry
            : DateTimeOffset.UtcNow.AddMinutes(4); // conservative fallback

        _cache.Set(RateSessionCacheKey, session, new MemoryCacheEntryOptions
        {
            AbsoluteExpiration = expiry
        });
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
                "GET products exception",
                $"SKU={sku} | {ex.Message}");
            return null;
        }
    }

    // ── Webhook ───────────────────────────────────────────────────────────

    public async Task<bool> ValidateWebhookSignatureAsync(byte[] payload, string signature)
    {
        var config = await _configRepo.GetConfigByTypeAsync(IntegrationType.Shiprelay);
        if (string.IsNullOrWhiteSpace(config?.WebhookSecret)) return false;

        var key = Encoding.UTF8.GetBytes(config.WebhookSecret);
        using var hmac = new HMACSHA256(key);
        var computed = hmac.ComputeHash(payload);

        var normalizedSignature = NormalizeWebhookSignature(signature);
        if (normalizedSignature == null) return false;

        byte[] received;
        try
        {
            received = Convert.FromHexString(normalizedSignature);
        }
        catch (FormatException)
        {
            return false;
        }

        return received.Length == computed.Length
            && CryptographicOperations.FixedTimeEquals(computed, received);
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    /// <summary>
    /// Builds the HTTP client (auth injected by ShiprelayClientHandler) and returns the reseller_id.
    /// ApiKey = login email, ApiSecret = login password, ResellerId = reseller_id UUID.
    /// </summary>
    private async Task<(HttpClient Client, string ResellerId)> BuildClientAsync()
    {
        var config = await _configRepo.GetConfigByTypeAsync(IntegrationType.Shiprelay)
            ?? throw new InvalidOperationException("Shiprelay integration is not configured or disabled");

        var client = _httpClientFactory.CreateClient("Shiprelay");
        client.BaseAddress = new Uri(config.BaseUrl.TrimEnd('/') + "/");
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("Accept", "application/json");

        return (client, config.ResellerId ?? string.Empty);
    }

    private static string? GetString(JsonElement el, string key)
        => el.TryGetProperty(key, out var prop) && prop.ValueKind == JsonValueKind.String
            ? prop.GetString()
            : null;

    /// <summary>
    /// Builds the full tracking URL from a ShipRelay JSON element.
    /// ShipRelay API v2 returns tracking info as a nested "tracking" object with
    /// "tracking_link" (base URL) and "tracking_number" (to append). Full URL = link + number.
    /// Falls back to flat "tracking_link"+"tracking_number", then legacy flat "tracking_url".
    /// </summary>
    private static string? BuildTrackingUrl(JsonElement el)
    {
        if (el.TryGetProperty("tracking", out var tracking) && tracking.ValueKind == JsonValueKind.Object)
        {
            var link = GetString(tracking, "tracking_link");
            var num  = GetString(tracking, "tracking_number");
            if (!string.IsNullOrEmpty(link))
                return string.IsNullOrEmpty(num) ? link : link + num;
        }
        var flatLink = GetString(el, "tracking_link");
        var flatNum  = GetString(el, "tracking_number");
        if (!string.IsNullOrEmpty(flatLink))
            return string.IsNullOrEmpty(flatNum) ? flatLink : flatLink + flatNum;
        return GetString(el, "tracking_url");
    }

    /// <summary>Extracts tracking number from nested "tracking" object or flat "tracking_number".</summary>
    private static string? GetTrackingNumberFrom(JsonElement el)
    {
        if (el.TryGetProperty("tracking", out var tracking) && tracking.ValueKind == JsonValueKind.Object)
            return GetString(tracking, "tracking_number") ?? GetString(el, "tracking_number");
        return GetString(el, "tracking_number");
    }

    /// <summary>Extracts carrier name from carrier field which may be a string or an object with a "name" property.</summary>
    private static string? ExtractCarrierName(JsonElement el)
    {
        if (!el.TryGetProperty("carrier", out var carrier)) return null;
        if (carrier.ValueKind == JsonValueKind.String) return carrier.GetString();
        if (carrier.ValueKind == JsonValueKind.Object) return GetString(carrier, "name");
        return null;
    }

    /// <summary>Extracts service name from carrier.service or top-level "service" field.</summary>
    private static string? ExtractServiceName(JsonElement el)
    {
        if (el.TryGetProperty("carrier", out var carrier) && carrier.ValueKind == JsonValueKind.Object)
        {
            var service = GetString(carrier, "service");
            if (!string.IsNullOrEmpty(service)) return service;
        }
        return GetString(el, "service");
    }

    private static string? NormalizeWebhookSignature(string signature)
    {
        if (string.IsNullOrWhiteSpace(signature)) return null;

        var value = signature.Trim();
        const string legacyPrefix = "sha256=";
        if (value.StartsWith(legacyPrefix, StringComparison.OrdinalIgnoreCase))
            value = value[legacyPrefix.Length..];

        return value;
    }

    private static string BuildShipmentsQueryString(ShiprelayGetShipmentsRequest r)
    {
        var parts = new List<string>
        {
            $"page={r.Page}",
            $"per_page={r.PerPage}"
        };
        if (!string.IsNullOrEmpty(r.SourceOrderId)) parts.Add($"source_order_id={Uri.EscapeDataString(r.SourceOrderId)}");
        if (!string.IsNullOrEmpty(r.SourceShipmentId)) parts.Add($"source_shipment_id={Uri.EscapeDataString(r.SourceShipmentId)}");
        if (!string.IsNullOrEmpty(r.OrderRef)) parts.Add($"order_ref={Uri.EscapeDataString(r.OrderRef)}");
        if (!string.IsNullOrEmpty(r.Status)) parts.Add($"status={Uri.EscapeDataString(r.Status)}");
        if (!string.IsNullOrEmpty(r.TrackingNumber)) parts.Add($"tracking_number={Uri.EscapeDataString(r.TrackingNumber)}");
        if (!string.IsNullOrEmpty(r.UpdatedAtFrom)) parts.Add($"updated_at_from={Uri.EscapeDataString(r.UpdatedAtFrom)}");
        if (!string.IsNullOrEmpty(r.UpdatedAtTo)) parts.Add($"updated_at_to={Uri.EscapeDataString(r.UpdatedAtTo)}");
        return "?" + string.Join("&", parts);
    }
}
