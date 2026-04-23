using LumStoreAPI.Core.Entities.Customers;

namespace LumStoreAPI.Infrastructure.Repositories.Interfaces;

public interface ICartRepository
{
    Task<IEnumerable<UserCart>> GetByUserIdAsync(int userId, CancellationToken ct = default);
    Task<UserCart?> GetByIdAsync(int cartItemId, CancellationToken ct = default);
    Task<UserCart?> GetExistingItemAsync(int userId, int nodeId, int? variantId, CancellationToken ct = default);
    Task<UserCart> AddAsync(UserCart item, CancellationToken ct = default);
    Task UpdateQuantityAsync(int cartItemId, int quantity, CancellationToken ct = default);
    Task RemoveAsync(int cartItemId, CancellationToken ct = default);
    Task ClearAsync(int userId, CancellationToken ct = default);
}
