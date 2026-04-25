using LumStoreAPI.Core.Entities.Customers;
using LumStoreAPI.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LumStoreAPI.Infrastructure.Repositories.Presentations;

internal class CartRepository : ICartRepository
{
    private readonly LumStoreContext _ctx;

    public CartRepository(LumStoreContext ctx) => _ctx = ctx;

    public async Task<IEnumerable<UserCart>> GetByUserIdAsync(int userId, CancellationToken ct = default)
        => await _ctx.UserCarts
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct);

    public Task<UserCart?> GetByIdAsync(int cartItemId, CancellationToken ct = default)
        => _ctx.UserCarts.FirstOrDefaultAsync(x => x.Id == cartItemId, ct);

    public Task<UserCart?> GetExistingItemAsync(int userId, int nodeId, int? variantId, CancellationToken ct = default)
        => _ctx.UserCarts.FirstOrDefaultAsync(
            x => x.UserId == userId && x.NodeId == nodeId && x.VariantId == variantId, ct);

    public async Task<UserCart> AddAsync(UserCart item, CancellationToken ct = default)
    {
        _ctx.UserCarts.Add(item);
        await _ctx.SaveChangesAsync(ct);
        return item;
    }

    public async Task UpdateQuantityAsync(int cartItemId, int quantity, CancellationToken ct = default)
        => await _ctx.UserCarts
            .Where(x => x.Id == cartItemId)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.Quantity, quantity), ct);

    public async Task RemoveAsync(int cartItemId, CancellationToken ct = default)
        => await _ctx.UserCarts.Where(x => x.Id == cartItemId).ExecuteDeleteAsync(ct);

    public async Task ClearAsync(int userId, CancellationToken ct = default)
        => await _ctx.UserCarts.Where(x => x.UserId == userId).ExecuteDeleteAsync(ct);
}
