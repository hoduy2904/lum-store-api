namespace LumStoreAPI.Infrastructure.Repositories.Interfaces;

public interface IWishlistRepository
{
    Task<IEnumerable<int>> GetNodeIdsAsync(int userId, CancellationToken ct = default);
    Task AddAsync(int userId, int nodeId, CancellationToken ct = default);
    Task RemoveAsync(int userId, int nodeId, CancellationToken ct = default);
    Task BulkAddAsync(int userId, IEnumerable<int> nodeIds, CancellationToken ct = default);
}
