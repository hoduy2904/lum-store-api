using LumStoreAPI.Application.DTOs.OrderDTO;
using LumStoreAPI.Application.DTOs.StoreOrderDTO;
using LumStoreAPI.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LumStoreAPI.ClientControllers;

[Route("api/store/orders")]
[ApiController]
[Authorize]
public class StoreOrderController(IStoreOrderService storeOrderService) : ControllerBase
{
    private readonly IStoreOrderService _storeOrderService = storeOrderService;

    [HttpPost]
    public async Task<IActionResult> PlaceOrder([FromBody] StorePlaceOrderRequest request, CancellationToken ct)
    {
        var result = await _storeOrderService.PlaceOrderAsync(request, ct);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? status = null)
    {
        var result = await _storeOrderService.GetOrdersAsync(page, pageSize, status, CancellationToken.None);
        return Ok(result);
    }

    [HttpGet("{orderId:int}")]
    public async Task<IActionResult> GetOrder(int orderId)
    {
        var result = await _storeOrderService.GetOrderAsync(orderId, CancellationToken.None);
        if (!result.IsSuccess) return result.Error == "Forbidden" ? Forbid() : NotFound(result);
        return Ok(result);
    }

    /// <summary>POST /api/store/orders/{orderId}/cancel — Cancel own order (Pending or Confirmed only).</summary>
    [HttpPatch("{orderId:int}/cancel")]
    public async Task<IActionResult> CancelOrder(int orderId, [FromBody] StoreCancelOrderRequest request, CancellationToken ct)
    {
        var result = await _storeOrderService.CancelOrderAsync(orderId, request, ct);
        if (!result.IsSuccess) return result.Error is "Forbidden" ? Forbid() : BadRequest(result);
        return Ok(result);
    }

    /// <summary>GET /api/store/orders/{orderId}/tracking — Live tracking info for an order.</summary>
    [HttpGet("{orderId:int}/tracking")]
    public async Task<IActionResult> GetTracking(int orderId, CancellationToken ct)
    {
        var result = await _storeOrderService.GetTrackingAsync(orderId, ct);
        if (!result.IsSuccess) return result.Error is "Forbidden" ? Forbid() : NotFound(result);
        return Ok(result);
    }

    /// <summary>PATCH /api/store/orders/{orderId}/return — Request a return for a delivered + paid order.</summary>
    [HttpPatch("{orderId:int}/return")]
    public async Task<IActionResult> RequestReturn(int orderId, [FromBody] StoreRequestReturnRequest request, CancellationToken ct)
    {
        var result = await _storeOrderService.RequestReturnAsync(orderId, request, ct);
        if (!result.IsSuccess) return result.Error is "Forbidden" ? Forbid() : BadRequest(result);
        return Ok(result);
    }

    /// <summary>GET /api/store/orders/{orderId}/returns — List returns for own order.</summary>
    [HttpGet("{orderId:int}/returns")]
    public async Task<IActionResult> GetOrderReturns(int orderId, CancellationToken ct)
    {
        var result = await _storeOrderService.GetOrderReturnsAsync(orderId, ct);
        if (!result.IsSuccess) return result.Error == "Forbidden" ? Forbid() : NotFound(result);
        return Ok(result);
    }

    /// <summary>POST /api/store/orders/{orderId}/returns — Submit a return request.</summary>
    [HttpPost("{orderId:int}/returns")]
    public async Task<IActionResult> SubmitReturn(int orderId, [FromBody] OrderReturnCreateDTO dto, CancellationToken ct)
    {
        var result = await _storeOrderService.SubmitReturnAsync(orderId, dto, ct);
        if (!result.IsSuccess) return result.Error == "Forbidden" ? Forbid() : BadRequest(result);
        return Ok(result);
    }

    /// <summary>GET /api/store/orders/{orderId}/payment-status — Poll current payment status. Use after Stripe redirect to confirm payment before showing success UI.</summary>
    [HttpGet("{orderId:int}/payment-status")]
    public async Task<IActionResult> CheckPaymentStatus(int orderId, CancellationToken ct)
    {
        var result = await _storeOrderService.CheckPaymentStatusAsync(orderId, ct);
        if (!result.IsSuccess) return result.Error == "Forbidden" ? Forbid() : NotFound(result);
        return Ok(result);
    }
}
