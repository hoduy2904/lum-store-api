using LumStoreAPI.Application.DTOs.CartDTO;
using LumStoreAPI.Application.DTOs.ProductComboDTO;
using LumStoreAPI.Application.DTOs.ProductDTO;
using LumStoreAPI.Application.DTOs.Responses;
using LumStoreAPI.Application.Interfaces;
using LumStoreAPI.Core.Entities.Customers;
using LumStoreAPI.Infrastructure;
using LumStoreAPI.Infrastructure.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace LumStoreAPI.Application.Services;

internal class CartService : ICartService
{
    private readonly ICartRepository _cartRepo;
    private readonly IProductService _productService;
    private readonly LumStoreContext _ctx;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IDiscountRuleService _discountRuleService;
    private readonly IProductVariantService _productVariantService;

    public CartService(
        ICartRepository cartRepo,
        IProductService productService,
        LumStoreContext ctx,
        IHttpContextAccessor httpContextAccessor,
        IDiscountRuleService discountRuleService,
        IProductVariantService productVariantService)
    {
        _cartRepo = cartRepo;
        _productService = productService;
        _ctx = ctx;
        _httpContextAccessor = httpContextAccessor;
        _discountRuleService = discountRuleService;
        _productVariantService = productVariantService;
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
        => _ctx.Products.AnyAsync(n => n.NodeID == nodeId &&
            (n.IsCombo || n.ProductVariants.Any(v => v.ShiprelayId > 0)), ct);

    private async Task<int?> GetStockAsync(int? variantId, CancellationToken ct)
    {
        if (variantId is null) return null;
        return await _ctx.ProductVariants
            .Where(v => v.ShiprelayId > 0 && v.ItemID == variantId)
            .Select(v => (int?)v.Stock)
            .FirstOrDefaultAsync(ct);
    }

    private async Task<int?> GetComboStockAsync(int nodeId, CancellationToken ct)
    {
        var isCombo = await _ctx.Products
            .Where(p => p.NodeID == nodeId && p.IsCombo)
            .AnyAsync();

        if (isCombo != true) return null;

        var comboResult = await _discountRuleService.CalculateComboPriceAsync(nodeId);
        return comboResult.ComboStock;
    }

    public async Task<CartResponseDTO> BuildCartResponseAsync(int userId, Action<Dictionary<int, ComboPriceResult>, CartItemDTO>? action = null, CancellationToken ct = default)
    {
        var cartItems = (await _cartRepo.GetByUserIdAsync(userId, ct)).ToList();
        if (cartItems.Count == 0) return new CartResponseDTO();

        var variantIds = cartItems.Where(x => x.VariantId is not null).Select(x => x.VariantId!.Value).ToArray();
        var expandNodeIds = await _productVariantService.GetVariantIdAndProductIdsAsync(variantIds);
        var nodeIds = cartItems.Select(x => x.NodeId).Union(expandNodeIds.Values).Distinct().ToArray();
        var products = (await _productService.GetProductsByNodeIdsAsync(nodeIds))
            .ToDictionary(p => p.NodeID);

        // Extract price data from already-loaded products — avoids a redundant DB round-trip
        var prices = products
           .Where(kvp => kvp.Value.Fields is ProductClientDTO f && f != null)
           .ToDictionary(
            kvp => kvp.Key,
            kvp =>
            {
                var f = (kvp.Value.Fields as ProductClientDTO)!;
                return new
                {
                    Price = f.Price ?? 0m,
                    PriceDiscount = f.PriceDiscount ?? 0m,
                    Variants = f.ProductVariants,
                    IsCombo = f.IsCombo,
                    IsExpand = f.IsExpand,
                    DiscountRules = f.DiscountRules
                };
            });

        var items = cartItems.Select(c =>
        {
            products.TryGetValue(c.NodeId, out var product);
            return new CartItemDTO
            {
                CartItemId = c.Id,
                NodeID = c.NodeId,
                Quantity = c.Quantity,
                VariantId = c.VariantId,
                Product = product,
            };
        }).ToList();

        // Batch-load combo prices in 2 queries total regardless of combo count
        var comboNodeIds = nodeIds.Where(id => prices.TryGetValue(id, out var p) && p.IsCombo).Distinct().ToArray();
        var comboPriceResults = comboNodeIds.Length > 0
            ? (await _discountRuleService.CalculateBatchComboPricesAsync(comboNodeIds))
            : [];

        var comboPrices = comboPriceResults
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.TotalPrice);

        var variantComboResults = comboPriceResults.Values.SelectMany(s => s.Items);

        decimal subtotal = 0;
        foreach (var item in items)
        {
            if (!prices.TryGetValue(item.NodeID, out var p)) continue;
            decimal unitPrice = 0m, basePrice = 0m, discountPrice = 0m;
            var variantPrice = prices.Values.FirstOrDefault(x => x.Variants.Any(v => v.VariantId == item.VariantId));
            if (variantPrice != null)
            {
                basePrice = variantPrice.PriceDiscount > 0 ? variantPrice.PriceDiscount : variantPrice.Price;
                discountPrice = variantPrice.PriceDiscount;
            }
            else if (p.IsExpand)
            {
                var comboPrice = variantComboResults.FirstOrDefault(x => x.VariantId == item.VariantId);
                basePrice = comboPrice?.UnitPrice ?? 0m;
                discountPrice = comboPrice?.DiscountedPrice ?? 0m;
            }

            if (p.IsCombo)
            {
                // comboPrices already incorporates the combo-level discount rule — do not apply again
                basePrice = comboPrices.TryGetValue(item.NodeID, out var cp) ? cp : p.Price;
                unitPrice = basePrice;
            }
            else
            {
                unitPrice = ApplyBestDiscount(p.DiscountRules, basePrice, item.Quantity);
            }
            item.UnitPrice = unitPrice;
            item.BasePrice = basePrice;

            subtotal += unitPrice * item.Quantity;

            action?.Invoke(comboPriceResults, item);
        }

        return new CartResponseDTO
        {
            Items = items,
            Subtotal = subtotal
        };
    }

    // ── Queries ───────────────────────────────────────────────────────────────

    public async Task<APIResponse<CartResponseDTO>> GetCartAsync(CancellationToken ct = default)
    {
        var userId = GetCurrentUserId();
        return APIResponse<CartResponseDTO>.Success(await BuildCartResponseAsync(userId, ct: ct));
    }

    // ── Commands ──────────────────────────────────────────────────────────────

    public async Task<APIResponse<CartAddResultDTO>> AddToCartAsync(CartAddRequest request, CancellationToken ct = default)
    {
        var userId = GetCurrentUserId();

        if (!await IsValidProductNodeAsync(request.NodeID, ct))
            return APIResponse<CartAddResultDTO>.Failure("Product not found");

        if (request.VariantId.HasValue)
        {
            var stock = await GetStockAsync(request.VariantId, ct);
            if (stock is null)
                return APIResponse<CartAddResultDTO>.Failure("Variant not found");

            var existing = await _cartRepo.GetExistingItemAsync(userId, request.NodeID, request.VariantId, ct);
            var totalQty = (existing?.Quantity ?? 0) + request.Quantity;
            if (totalQty > stock)
                return APIResponse<CartAddResultDTO>.Failure($"Insufficient stock. Available: {stock}");
        }
        else
        {
            var comboStock = await GetComboStockAsync(request.NodeID, ct);
            if (comboStock.HasValue)
            {
                var existing = await _cartRepo.GetExistingItemAsync(userId, request.NodeID, null, ct);
                var totalQty = (existing?.Quantity ?? 0) + request.Quantity;
                if (totalQty > comboStock)
                    return APIResponse<CartAddResultDTO>.Failure($"Insufficient stock. Available: {comboStock}");
            }
        }

        var cartItem = await _cartRepo.GetExistingItemAsync(userId, request.NodeID, request.VariantId, ct);
        if (cartItem is not null)
        {
            var newQty = cartItem.Quantity + request.Quantity;
            await _cartRepo.UpdateQuantityAsync(cartItem.Id, newQty, ct);
            cartItem.Quantity = newQty;
        }
        else
        {
            cartItem = await _cartRepo.AddAsync(new UserCart
            {
                UserId = userId,
                NodeId = request.NodeID,
                VariantId = request.VariantId,
                Quantity = request.Quantity
            }, ct);
        }

        return APIResponse<CartAddResultDTO>.Success(new CartAddResultDTO
        {
            CartItemId = cartItem.Id,
            NodeID = cartItem.NodeId,
            Quantity = cartItem.Quantity,
            VariantId = cartItem.VariantId
        }, ["Added to cart"]);
    }

    public async Task<APIResponseBase> UpdateQuantityAsync(int cartItemId, CartUpdateRequest request, CancellationToken ct = default)
    {
        var userId = GetCurrentUserId();
        var item = await _cartRepo.GetByIdAsync(cartItemId, ct);

        if (item is null || item.UserId != userId)
            return APIResponseBase.Failure("Cart item not found");

        if (item.VariantId.HasValue)
        {
            var stock = await GetStockAsync(item.VariantId, ct);
            if (stock.HasValue && request.Quantity > stock)
                return APIResponseBase.Failure($"Insufficient stock. Available: {stock}");
        }
        else
        {
            var comboStock = await GetComboStockAsync(item.NodeId, ct);
            if (comboStock.HasValue && request.Quantity > comboStock)
                return APIResponseBase.Failure($"Insufficient stock. Available: {comboStock}");
        }

        await _cartRepo.UpdateQuantityAsync(cartItemId, request.Quantity, ct);
        return APIResponseBase.Success(["Cart updated"]);
    }

    public async Task<APIResponseBase> RemoveFromCartAsync(int cartItemId, CancellationToken ct = default)
    {
        var userId = GetCurrentUserId();
        var item = await _cartRepo.GetByIdAsync(cartItemId, ct);

        if (item is null) return APIResponseBase.Success(["Item removed"]);

        if (item.UserId != userId)
            return APIResponseBase.Failure("Forbidden");

        await _cartRepo.RemoveAsync(cartItemId, ct);
        return APIResponseBase.Success(["Item removed"]);
    }

    public async Task<APIResponseBase> ClearCartAsync(CancellationToken ct = default)
    {
        var userId = GetCurrentUserId();
        await _cartRepo.ClearAsync(userId, ct);
        return APIResponseBase.Success(["Cart cleared"]);
    }

    // ── Helpers (pure, no DB) ─────────────────────────────────────────────────

    /// <summary>
    /// Replicates GetBestRuleAsync logic in-memory using pre-loaded tiers.
    /// Formula: Max(0, Round(unitPrice × (1 − DiscountPercent/100) − DiscountAmount, 2))
    /// </summary>
    private static decimal ApplyBestDiscount(
        IEnumerable<ProductDiscountTierDTO> tiers,
        decimal unitPrice,
        int quantity)
    {
        var best = tiers
            .Where(t => t.MinQuantity <= quantity && (t.MaxQuantity == null || t.MaxQuantity >= quantity))
            .OrderByDescending(t => t.DiscountPercent + t.DiscountAmount)
            .FirstOrDefault();

        if (best is null) return unitPrice;
        return Math.Max(0, Math.Round(unitPrice * (1 - best.DiscountPercent / 100) - best.DiscountAmount, 2));
    }

    public async Task<APIResponse<CartResponseDTO>> SyncCartAsync(CartSyncRequest request, CancellationToken ct = default)
    {
        var userId = GetCurrentUserId();

        if (request.Items.Length > 0)
        {
            var incomingNodeIds = request.Items.Select(x => x.NodeID).ToArray();
            var validNodeIds = await _ctx.Products
                .Where(n => incomingNodeIds.Contains(n.NodeID) && (n.IsCombo || n.ProductVariants.Any(v => v.ShiprelayId > 0)))
                .Select(n => n.NodeID)
                .ToHashSetAsync(ct);

            // Pre-load all existing cart items in 1 query — eliminates N GetExistingItemAsync calls
            var existingItems = (await _cartRepo.GetByUserIdAsync(userId, ct))
                .ToDictionary(e => (e.NodeId, e.VariantId));

            foreach (var incoming in request.Items)
            {
                if (!validNodeIds.Contains(incoming.NodeID)) continue;

                if (existingItems.TryGetValue((incoming.NodeID, incoming.VariantId), out var existing))
                {
                    var merged = Math.Max(existing.Quantity, incoming.Quantity);
                    if (merged != existing.Quantity)
                        await _cartRepo.UpdateQuantityAsync(existing.Id, merged, ct);
                }
                else
                {
                    await _cartRepo.AddAsync(new UserCart
                    {
                        UserId = userId,
                        NodeId = incoming.NodeID,
                        VariantId = incoming.VariantId,
                        Quantity = incoming.Quantity
                    }, ct);
                }
            }
        }

        var cart = await BuildCartResponseAsync(userId, ct: ct);
        return APIResponse<CartResponseDTO>.Success(cart, [$"Synced {request.Items.Length} items"]);
    }
}
