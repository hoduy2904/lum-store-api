using LumStoreAPI.Application.DTOs.DocumentPageDTO;
using LumStoreAPI.Application.DTOs.Responses;
using LumStoreAPI.Application.DTOs.WishlistDTO;
using LumStoreAPI.Application.Interfaces;
using LumStoreAPI.Infrastructure;
using LumStoreAPI.Infrastructure.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace LumStoreAPI.Application.Services;

internal class WishlistService : IWishlistService
{
    private readonly IWishlistRepository _wishlistRepo;
    private readonly IProductService _productService;
    private readonly LumStoreContext _ctx;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public WishlistService(
        IWishlistRepository wishlistRepo,
        IProductService productService,
        LumStoreContext ctx,
        IHttpContextAccessor httpContextAccessor)
    {
        _wishlistRepo = wishlistRepo;
        _productService = productService;
        _ctx = ctx;
        _httpContextAccessor = httpContextAccessor;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private int GetCurrentUserId()
    {
        var claim = _httpContextAccessor.HttpContext?.User
            .Claims.FirstOrDefault(x => x.Type == "id")?.Value;

        if (!int.TryParse(claim, out int userId))
            throw new UnauthorizedAccessException("User identity could not be resolved.");

        return userId;
    }

    private Task<bool> IsValidProductNodeAsync(int nodeId, CancellationToken ct)
        => _ctx.DocumentNodes.AnyAsync(n => n.NodeID == nodeId && n.ClassName == "Pages.Product", ct);

    // ── Queries ───────────────────────────────────────────────────────────────

    public async Task<APIResponse<IEnumerable<DocumentClientGetDTO>>> GetWishlistAsync(CancellationToken ct = default)
    {
        var userId = GetCurrentUserId();
        var nodeIds = (await _wishlistRepo.GetNodeIdsAsync(userId, ct)).ToArray();

        if (nodeIds.Length == 0)
            return APIResponse<IEnumerable<DocumentClientGetDTO>>.Success([]);

        var products = await _productService.GetProductsByNodeIdsAsync(nodeIds);
        return APIResponse<IEnumerable<DocumentClientGetDTO>>.Success(products);
    }

    // ── Commands ──────────────────────────────────────────────────────────────

    public async Task<APIResponseBase> AddToWishlistAsync(WishlistAddRequest request, CancellationToken ct = default)
    {
        var userId = GetCurrentUserId();

        if (!await IsValidProductNodeAsync(request.NodeID, ct))
            return APIResponseBase.Failure("Product not found");

        await _wishlistRepo.AddAsync(userId, request.NodeID, ct);
        return APIResponseBase.Success(["Added to wishlist"]);
    }

    public async Task<APIResponseBase> RemoveFromWishlistAsync(int nodeId, CancellationToken ct = default)
    {
        var userId = GetCurrentUserId();
        await _wishlistRepo.RemoveAsync(userId, nodeId, ct);
        return APIResponseBase.Success(["Removed from wishlist"]);
    }

    public async Task<APIResponse<IEnumerable<DocumentClientGetDTO>>> SyncWishlistAsync(WishlistSyncRequest request, CancellationToken ct = default)
    {
        var userId = GetCurrentUserId();

        if (request.NodeIDs.Length > 0)
        {
            // Filter to only valid product nodes before bulk insert
            var validNodeIds = await _ctx.DocumentNodes
                .Where(n => request.NodeIDs.Contains(n.NodeID) && n.ClassName == "Pages.Product")
                .Select(n => n.NodeID)
                .ToListAsync(ct);

            await _wishlistRepo.BulkAddAsync(userId, validNodeIds, ct);
        }

        var nodeIds = (await _wishlistRepo.GetNodeIdsAsync(userId, ct)).ToArray();

        if (nodeIds.Length == 0)
            return APIResponse<IEnumerable<DocumentClientGetDTO>>.Success([], [$"Synced {request.NodeIDs.Length} items"]);

        var products = await _productService.GetProductsByNodeIdsAsync(nodeIds);
        return APIResponse<IEnumerable<DocumentClientGetDTO>>.Success(products, [$"Synced {request.NodeIDs.Length} items"]);
    }
}
