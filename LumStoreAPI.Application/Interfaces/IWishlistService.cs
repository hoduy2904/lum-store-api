using LumStoreAPI.Application.DTOs.DocumentPageDTO;
using LumStoreAPI.Application.DTOs.Responses;
using LumStoreAPI.Application.DTOs.WishlistDTO;

namespace LumStoreAPI.Application.Interfaces;

public interface IWishlistService
{
    Task<APIResponse<IEnumerable<DocumentClientGetDTO>>> GetWishlistAsync(CancellationToken ct = default);
    Task<APIResponseBase> AddToWishlistAsync(WishlistAddRequest request, CancellationToken ct = default);
    Task<APIResponseBase> RemoveFromWishlistAsync(int nodeId, CancellationToken ct = default);
    Task<APIResponse<IEnumerable<DocumentClientGetDTO>>> SyncWishlistAsync(WishlistSyncRequest request, CancellationToken ct = default);
}
