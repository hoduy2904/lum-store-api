using LumStoreAPI.Core.Entities.Customers;
using LumStoreAPI.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LumStoreAPI.Infrastructure.Repositories.Presentations;

internal class WishlistRepository : IWishlistRepository
{
    private readonly LumStoreContext _ctx;

    public WishlistRepository(LumStoreContext ctx) => _ctx = ctx;

    public async Task<IEnumerable<int>> GetNodeIdsAsync(int userId, CancellationToken ct = default)
        => await _ctx.UserWishlists
            .Where(w => w.UserId == userId)
            .OrderByDescending(w => w.CreatedAt)
            .Select(w => w.NodeId)
            .ToListAsync(ct);

    public async Task AddAsync(int userId, int nodeId, CancellationToken ct = default)
    {
        var exists = await _ctx.UserWishlists
            .AnyAsync(w => w.UserId == userId && w.NodeId == nodeId, ct);

        if (exists) return;

        _ctx.UserWishlists.Add(new UserWishlist { UserId = userId, NodeId = nodeId });
        await _ctx.SaveChangesAsync(ct);
    }

    public async Task RemoveAsync(int userId, int nodeId, CancellationToken ct = default)
    {
        var item = await _ctx.UserWishlists
            .FirstOrDefaultAsync(w => w.UserId == userId && w.NodeId == nodeId, ct);

        if (item is null) return;

        _ctx.UserWishlists.Remove(item);
        await _ctx.SaveChangesAsync(ct);
    }

    public async Task BulkAddAsync(int userId, IEnumerable<int> nodeIds, CancellationToken ct = default)
    {
        var incoming = nodeIds.Distinct().ToList();
        if (incoming.Count == 0) return;

        var existing = await _ctx.UserWishlists
            .Where(w => w.UserId == userId && incoming.Contains(w.NodeId))
            .Select(w => w.NodeId)
            .ToListAsync(ct);

        var toAdd = incoming
            .Except(existing)
            .Select(nodeId => new UserWishlist { UserId = userId, NodeId = nodeId });

        _ctx.UserWishlists.AddRange(toAdd);
        await _ctx.SaveChangesAsync(ct);
    }
}
