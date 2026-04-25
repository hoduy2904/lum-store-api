using LumStoreAPI.Application.DTOs.CartDTO;
using LumStoreAPI.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LumStoreAPI.ClientControllers;

[Route("api/store/cart")]
[ApiController]
[Authorize]
public class StoreCartController(ICartService cartService) : ControllerBase
{
    private readonly ICartService _cartService = cartService;

    [HttpGet]
    public async Task<IActionResult> GetCart()
    {
        var result = await _cartService.GetCartAsync(CancellationToken.None);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> AddToCart([FromBody] CartAddRequest request, CancellationToken ct)
    {
        var result = await _cartService.AddToCartAsync(request, ct);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    [HttpPut("{cartItemId:int}")]
    public async Task<IActionResult> UpdateQuantity(int cartItemId, [FromBody] CartUpdateRequest request, CancellationToken ct)
    {
        var result = await _cartService.UpdateQuantityAsync(cartItemId, request, ct);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    [HttpDelete("{cartItemId:int}")]
    public async Task<IActionResult> RemoveItem(int cartItemId, CancellationToken ct)
    {
        var result = await _cartService.RemoveFromCartAsync(cartItemId, ct);
        if (!result.IsSuccess) return StatusCode(403, result);
        return Ok(result);
    }

    [HttpDelete]
    public async Task<IActionResult> ClearCart(CancellationToken ct)
    {
        var result = await _cartService.ClearCartAsync(ct);
        return Ok(result);
    }

    [HttpPost("sync")]
    public async Task<IActionResult> SyncCart([FromBody] CartSyncRequest request, CancellationToken ct)
    {
        var result = await _cartService.SyncCartAsync(request, ct);
        return Ok(result);
    }
}
