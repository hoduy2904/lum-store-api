using LumStoreAPI.Application.DTOs.CartDTO;
using LumStoreAPI.Application.DTOs.ProductComboDTO;
using LumStoreAPI.Application.DTOs.ProductVariantDTO;
using LumStoreAPI.Application.DTOs.Responses;

namespace LumStoreAPI.Application.Interfaces;

public interface ICartService
{
    Task<APIResponse<CartResponseDTO>> GetCartAsync(CancellationToken ct = default);
    Task<APIResponse<CartAddResultDTO>> AddToCartAsync(CartAddRequest request, CancellationToken ct = default);
    Task<APIResponseBase> UpdateQuantityAsync(int cartItemId, CartUpdateRequest request, CancellationToken ct = default);
    Task<APIResponseBase> RemoveFromCartAsync(int cartItemId, CancellationToken ct = default);
    Task<APIResponseBase> ClearCartAsync(CancellationToken ct = default);
    Task<APIResponse<CartResponseDTO>> SyncCartAsync(CartSyncRequest request, CancellationToken ct = default);

    Task<CartResponseDTO> BuildCartResponseAsync(int userId,
        Action<Dictionary<int, ComboPriceResult>, CartItemDTO, IEnumerable<ProductVariantClientGetDTO>>? action = null,
        CancellationToken ct = default);
}
