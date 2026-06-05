using LumStoreAPI.Application.DTOs.IntegrationDTO;
using LumStoreAPI.Application.DTOs.OrderDTO;
using LumStoreAPI.Application.DTOs.Responses;
using LumStoreAPI.Application.Interfaces;
using LumStoreAPI.Core.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LumStoreAPI.Controllers;

/// <summary>Integration configuration management (Shiprelay, WMS, etc.).</summary>
[Route("api/integrations")]
[ApiController]
[Authorize(Roles = "ADMIN")]
public class IntegrationController : ControllerBase
{
    private readonly IIntegrationConfigService _integrationService;

    public IntegrationController(IIntegrationConfigService integrationService)
        => _integrationService = integrationService;

    /// <summary>GET /api/integrations — List all integration configs.</summary>
    [HttpGet]
    public async Task<IActionResult> GetConfigs()
    {
        var configs = await _integrationService.GetConfigsAsync();
        return Ok(APIResponse<IEnumerable<IntegrationConfigGetDTO>>.Success(configs));
    }

    /// <summary>GET /api/integrations/{type} — Get config by integration type.</summary>
    [HttpGet("{type}")]
    public async Task<IActionResult> GetConfig(IntegrationType type)
    {
        var config = await _integrationService.GetConfigAsync(type);
        if (config == null) return NotFound(APIResponseBase.Failure("NOT_FOUND", [$"Config for {type} not found"]));
        return Ok(APIResponse<IntegrationConfigGetDTO>.Success(config));
    }

    /// <summary>PUT /api/integrations — Upsert integration config.</summary>
    [HttpPut]
    public async Task<IActionResult> UpsertConfig([FromBody] IntegrationConfigUpsertDTO dto)
    {
        var config = await _integrationService.UpsertConfigAsync(dto);
        return Ok(APIResponse<IntegrationConfigGetDTO>.Success(config, ["Config saved"]));
    }

    /// <summary>DELETE /api/integrations/{configId}</summary>
    [HttpDelete("{configId:int}")]
    public async Task<IActionResult> DeleteConfig(int configId)
    {
        var deleted = await _integrationService.DeleteConfigAsync(configId);
        if (!deleted) return NotFound(APIResponseBase.Failure("NOT_FOUND", ["Config not found"]));
        return Ok(APIResponseBase.Success(["Config deleted"]));
    }

    /// <summary>GET /api/integrations/sync-logs — Recent sync logs.</summary>
    [HttpGet("sync-logs")]
    public async Task<IActionResult> GetSyncLogs([FromQuery] IntegrationType? type = null)
    {
        var logs = await _integrationService.GetSyncLogsAsync(type);
        return Ok(APIResponse<IEnumerable<SyncLogGetDTO>>.Success(logs));
    }

    /// <summary>POST /api/integrations/{type}/sync — Manually trigger sync.</summary>
    [HttpPost("{type}/sync")]
    public async Task<IActionResult> TriggerSync(IntegrationType type)
    {
        int.TryParse(User.Claims.FirstOrDefault(c => c.Type == "id")?.Value, out int userId);
        var log = await _integrationService.TriggerSyncAsync(type, userId > 0 ? userId : null);
        return Ok(APIResponse<SyncLogGetDTO>.Success(log, ["Sync triggered"]));
    }

    /// <summary>POST /api/integrations/orders/{orderId}/ship — Create Shiprelay shipment manually.</summary>
    [HttpPost("orders/{orderId:int}/ship")]
    public async Task<IActionResult> CreateShipment(int orderId,
        [FromServices] IShiprelayService shiprelayService,
        [FromServices] IOrderService orderService,
        [FromBody] CreateShipmentRequest request)
    {
        var result = await shiprelayService.CreateShipmentAsync(new()
        {
            OrderId = orderId,
            OrderRef = request.OrderRef,
            ShipmentTotalCost = request.ShipmentTotalCost,
            PackageRef = request.PackageRef,
            Type = request.Type,
            RecipientName = request.RecipientName,
            Company = request.Company,
            Address1 = request.Address1,
            Address2 = request.Address2,
            City = request.City,
            State = request.State,
            Zip = request.Zip,
            Country = request.Country,
            Phone = request.Phone,
            Email = request.Email,
            Notes = request.Notes,
            Items = request.Items,
            ShippingSelectedRef = request.ShippingSelectedRef
        });
        if (!result.Success)
            return BadRequest(APIResponseBase.Failure("SHIPRELAY_ERROR", [result.ErrorMessage ?? "Failed to create shipment"]));

        // Persist ShipmentId + tracking back to the Order
        int? operatorId = int.TryParse(User.Claims.FirstOrDefault(c => c.Type == "id")?.Value, out int uid) ? uid : null;
        await orderService.UpdateOrderTrackingAsync(orderId, new()
        {
            ShiprelayShipmentId = result.ShipmentId,
            TrackingNumber = result.TrackingNumber,
            TrackingUrl = result.TrackingUrl,
            ShippingCarrier = result.Carrier
        }, operatorId);

        return Ok(APIResponse<object>.Success(result, ["Shipment created"]));
    }

    /// <summary>POST /api/integrations/orders/{orderId}/restore-shipment — Restore an archived ShipRelay shipment and set order back to Confirmed.</summary>
    [HttpPost("orders/{orderId:int}/restore-shipment")]
    public async Task<IActionResult> RestoreShipment(int orderId,
        [FromServices] IShiprelayService shiprelayService,
        [FromServices] IOrderService orderService)
    {
        var order = await orderService.GetOrderAsync(orderId);
        if (order is null)
            return NotFound(APIResponseBase.Failure("NOT_FOUND", [$"Order {orderId} not found"]));

        if (string.IsNullOrEmpty(order.ShiprelayShipmentId))
            return BadRequest(APIResponseBase.Failure("NO_SHIPMENT", ["Order has no ShipRelay shipment linked"]));

        var restored = await shiprelayService.RestoreShipmentAsync(order.ShiprelayShipmentId);
        if (!restored)
            return BadRequest(APIResponseBase.Failure("SHIPRELAY_ERROR", ["Failed to restore ShipRelay shipment"]));

        int? operatorId = int.TryParse(User.Claims.FirstOrDefault(c => c.Type == "id")?.Value, out int uid) ? uid : null;
        var updated = await orderService.UpdateOrderStatusAsync(orderId, new OrderUpdateStatusDTO
        {
            NewStatus = Core.Models.Enums.OrderStatus.Confirmed,
            Comment = $"Shipment {order.ShiprelayShipmentId} restored on ShipRelay"
        }, operatorId);

        return Ok(APIResponse<OrderGetDTO>.Success(updated, ["Shipment restored and order set to Confirmed"]));
    }

    /// <summary>POST /api/integrations/orders/{orderId}/sync-shiprelay — Pull latest shipment status from ShipRelay and update the order.</summary>
    [HttpPost("orders/{orderId:int}/sync-shiprelay")]
    public async Task<IActionResult> SyncFromShiprelay(int orderId,
        [FromServices] IOrderService orderService)
    {
        var order = await orderService.GetOrderAsync(orderId);
        if (order is null)
            return NotFound(APIResponseBase.Failure("NOT_FOUND", [$"Order {orderId} not found"]));

        if (string.IsNullOrEmpty(order.ShiprelayShipmentId))
            return BadRequest(APIResponseBase.Failure("NO_SHIPMENT", ["This order has no ShipRelay shipment linked"]));

        var result = await orderService.SyncOrderFromShiprelayAsync(orderId);
        if (result is null)
            return BadRequest(APIResponseBase.Failure("SHIPRELAY_ERROR", ["Could not retrieve shipment from ShipRelay"]));

        return Ok(APIResponse<OrderGetDTO>.Success(result, ["Order synced successfully"]));
    }

    /// <summary>POST /api/integrations/orders/{orderId}/rates — Get shipping rate estimates.</summary>
    [HttpPost("orders/{orderId:int}/rates")]
    public async Task<IActionResult> GetRates(int orderId, [FromServices] IShiprelayService shiprelayService,
        [FromBody] GetRatesRequest request)
    {
        var rates = await shiprelayService.GetRatesAsync(new()
        {
            OrderId = orderId,
            RecipientName = request.RecipientName,
            Address1 = request.Address1,
            Address2 = request.Address2,
            City = request.City,
            Region = request.Region,
            Country = request.Country ?? "US",
            Zip = request.Zip,
            Company = request.Company,
            Phone = request.Phone,
            Email = request.Email,
            SessionId = request.SessionId,
            Items = request.Items
        });
        return Ok(APIResponse<object>.Success(rates));
    }
}
