using LumStoreAPI.Application.DTOs.WishlistDTO;
using LumStoreAPI.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LumStoreAPI.ClientControllers;

[Route("api/store/wishlist")]
[ApiController]
[Authorize]
public class StoreWishlistController(IWishlistService wishlistService) : ControllerBase
{
    private readonly IWishlistService _wishlistService = wishlistService;

    [HttpGet]
    public async Task<IActionResult> GetWishlist(CancellationToken ct)
    {
        var result = await _wishlistService.GetWishlistAsync(ct);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> AddToWishlist([FromBody] WishlistAddRequest request, CancellationToken ct)
    {
        var result = await _wishlistService.AddToWishlistAsync(request, ct);
        if (!result.IsSuccess) return NotFound(result);
        return Ok(result);
    }

    [HttpDelete("{nodeId:int}")]
    public async Task<IActionResult> RemoveFromWishlist(int nodeId, CancellationToken ct)
    {
        var result = await _wishlistService.RemoveFromWishlistAsync(nodeId, ct);
        return Ok(result);
    }

    [HttpPost("sync")]
    public async Task<IActionResult> SyncWishlist([FromBody] WishlistSyncRequest request, CancellationToken ct)
    {
        var result = await _wishlistService.SyncWishlistAsync(request, ct);
        return Ok(result);
    }
}
