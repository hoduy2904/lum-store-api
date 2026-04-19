using LumStoreAPI.Core.Entities.Customers;
using LumStoreAPI.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LumStoreAPI.Infrastructure.Repositories.Presentations;

internal class CustomerAddressRepository : ICustomerAddressRepository
{
    private readonly LumStoreContext _ctx;

    public CustomerAddressRepository(LumStoreContext ctx) => _ctx = ctx;

    public async Task<IEnumerable<CustomerAddress>> GetByUserIdAsync(int userId, CancellationToken ct = default)
        => await _ctx.CustomerAddresses
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.IsDefault)
            .ThenBy(a => a.CreatedAt)
            .ToListAsync(ct);

    public Task<CustomerAddress?> GetByIdAsync(int id, CancellationToken ct = default)
        => _ctx.CustomerAddresses.FirstOrDefaultAsync(a => a.ItemID == id, ct);

    public async Task<CustomerAddress> InsertAsync(CustomerAddress address, CancellationToken ct = default)
    {
        _ctx.CustomerAddresses.Add(address);
        await _ctx.SaveChangesAsync(ct);
        return address;
    }

    public async Task<CustomerAddress> UpdateAsync(int id, Action<CustomerAddress> update, CancellationToken ct = default)
    {
        var address = await _ctx.CustomerAddresses.FirstAsync(a => a.ItemID == id, ct);
        update(address);
        await _ctx.SaveChangesAsync(ct);
        return address;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var address = await _ctx.CustomerAddresses.FirstOrDefaultAsync(a => a.ItemID == id, ct);
        if (address is null) return false;

        _ctx.CustomerAddresses.Remove(address);
        await _ctx.SaveChangesAsync(ct);
        return true;
    }

    public async Task SetDefaultAsync(int userId, int addressId, CancellationToken ct = default)
    {
        await using var tx = await _ctx.Database.BeginTransactionAsync(ct);
        try
        {
            // Clear all defaults for this user
            await _ctx.CustomerAddresses
                .Where(a => a.UserId == userId && a.IsDefault)
                .ExecuteUpdateAsync(s => s.SetProperty(a => a.IsDefault, false), ct);

            // Promote the target address
            await _ctx.CustomerAddresses
                .Where(a => a.ItemID == addressId && a.UserId == userId)
                .ExecuteUpdateAsync(s => s.SetProperty(a => a.IsDefault, true), ct);

            await tx.CommitAsync(ct);
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }

    public async Task ClearDefaultsAsync(int userId, int? excludeId = null, CancellationToken ct = default)
        => await _ctx.CustomerAddresses
            .Where(a => a.UserId == userId && a.IsDefault && (excludeId == null || a.ItemID != excludeId))
            .ExecuteUpdateAsync(s => s.SetProperty(a => a.IsDefault, false), ct);

    public Task<bool> ExistsForUserAsync(int id, int userId, CancellationToken ct = default)
        => _ctx.CustomerAddresses.AnyAsync(a => a.ItemID == id && a.UserId == userId, ct);
}
