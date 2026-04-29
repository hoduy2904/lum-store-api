using LumStoreAPI.Application.DTOs.StoreOrderDTO;
using LumStoreAPI.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LumStoreAPI.ClientControllers;

[Route("api/store/checkout")]
[ApiController]
[Authorize]
public class StoreCheckoutController(IStoreOrderService storeOrderService) : ControllerBase
{
    /// <summary>
    /// POST /api/store/checkout/preview — Preview order totals (subtotal, shipping, tax)
    /// for the current cart + selected address without placing the order.
    /// </summary>
    [HttpPost("preview")]
    public async Task<IActionResult> Preview([FromBody] StoreCheckoutPreviewRequest request, CancellationToken ct)
    {
        var result = await storeOrderService.GetCheckoutPreviewAsync(request, ct);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }
}
