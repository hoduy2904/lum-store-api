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
    public async Task<IActionResult> GetOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default)
    {
        var result = await _storeOrderService.GetOrdersAsync(page, pageSize, ct);
        return Ok(result);
    }

    [HttpGet("{orderId:int}")]
    public async Task<IActionResult> GetOrder(int orderId, CancellationToken ct)
    {
        var result = await _storeOrderService.GetOrderAsync(orderId, ct);
        if (!result.IsSuccess) return result.Error == "Forbidden" ? Forbid() : NotFound(result);
        return Ok(result);
    }
}
