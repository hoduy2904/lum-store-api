using LumStoreAPI.Core.Entities.Customers;
using LumStoreAPI.Core.Entities.Systems;
using LumStoreAPI.Core.Interfaces.ContentEngine;
using LumStoreAPI.Core.Models.Enums;
using LumStoreAPI.Infrastructure.Repositories.Interfaces;
using LumStoreAPI.Infrastructure.Types;
using Microsoft.EntityFrameworkCore;

namespace LumStoreAPI.Infrastructure.Repositories.Presentations;

internal class CustomerRepository : ICustomerRepository
{
    private readonly LumStoreContext _ctx;
    public CustomerRepository(LumStoreContext ctx) => _ctx = ctx;

    public Task<CustomerProfile?> GetProfileAsync(int profileId)
        => _ctx.CustomerProfiles
               .Include(p => p.User)
               .FirstOrDefaultAsync(p => p.ItemID == profileId);

    public Task<CustomerProfile?> GetProfileByUserIdAsync(int userId)
        => _ctx.CustomerProfiles
               .Include(p => p.User)
               .FirstOrDefaultAsync(p => p.UserId == userId);

    public async Task<IPagedEnumerable<CustomerProfile>> GetProfilesAsync(
        int page, int pageSize,
        string? search = null,
        CustomerTierLevel? tierLevel = null,
        string sortBy = "CreatedAt",
        bool descending = true)
    {
        IQueryable<CustomerProfile> query = _ctx.CustomerProfiles.Include(p => p.User);

        if (tierLevel.HasValue) query = query.Where(p => p.TierLevel == tierLevel.Value);
        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim().ToLower();
            query = query.Where(p =>
                p.User.UserName.ToLower().Contains(search) ||
                p.User.Email.ToLower().Contains(search) ||
                (p.User.FirstName + " " + p.User.LastName).ToLower().Contains(search));
        }

        query = sortBy switch
        {
            "TotalSpent" => descending ? query.OrderByDescending(p => p.TotalSpent) : query.OrderBy(p => p.TotalSpent),
            "TotalOrders" => descending ? query.OrderByDescending(p => p.TotalOrders) : query.OrderBy(p => p.TotalOrders),
            "TotalPoints" => descending ? query.OrderByDescending(p => p.TotalPoints) : query.OrderBy(p => p.TotalPoints),
            "TierLevel" => descending ? query.OrderByDescending(p => p.TierLevel) : query.OrderBy(p => p.TierLevel),
            _ => descending ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt)
        };

        int total = await query.CountAsync();
        var data = await query.Skip((page - 1) * pageSize).Take(pageSize).ToArrayAsync();
        return new PagedEnumerable<CustomerProfile>(data, total);
    }

    public async Task<CustomerProfile> InsertProfileAsync(CustomerProfile profile)
    {
        _ctx.CustomerProfiles.Add(profile);
        await _ctx.SaveChangesAsync();
        return profile;
    }

    public async Task<CustomerProfile> UpdateProfileAsync(int profileId, Action<CustomerProfile> update)
    {
        var profile = await _ctx.CustomerProfiles
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.ItemID == profileId)
            ?? throw new KeyNotFoundException($"CustomerProfile {profileId} not found");
        update(profile);
        await _ctx.SaveChangesAsync();
        return profile;
    }

    // ── Notes ─────────────────────────────────────────────────────────────

    public Task<IEnumerable<CustomerNote>> GetCustomerNotesAsync(int profileId)
        => Task.FromResult<IEnumerable<CustomerNote>>(
            _ctx.CustomerNotes
                .Where(n => n.CustomerProfileId == profileId)
                .OrderByDescending(n => n.CreatedAt)
                .AsEnumerable());

    public async Task<CustomerNote> InsertCustomerNoteAsync(CustomerNote note)
    {
        _ctx.CustomerNotes.Add(note);
        await _ctx.SaveChangesAsync();
        return note;
    }

    public async Task<bool> DeleteCustomerNoteAsync(int noteId)
    {
        var note = await _ctx.CustomerNotes.FindAsync(noteId);
        if (note == null) return false;
        _ctx.CustomerNotes.Remove(note);
        await _ctx.SaveChangesAsync();
        return true;
    }

    // ── Loyalty Points ────────────────────────────────────────────────────

    public Task<IEnumerable<LoyaltyPoint>> GetLoyaltyPointsAsync(int profileId)
        => Task.FromResult<IEnumerable<LoyaltyPoint>>(
            _ctx.LoyaltyPoints
                .Where(lp => lp.CustomerProfileId == profileId)
                .OrderByDescending(lp => lp.CreatedAt)
                .AsEnumerable());

    public async Task<LoyaltyPoint> InsertLoyaltyPointAsync(LoyaltyPoint point)
    {
        _ctx.LoyaltyPoints.Add(point);
        await _ctx.SaveChangesAsync();
        return point;
    }

    // ── Tiers ─────────────────────────────────────────────────────────────

    public async Task<IEnumerable<CustomerTier>> GetTiersAsync()
        => await _ctx.CustomerTiers.OrderBy(t => t.TierLevel).ToListAsync();

    public Task<CustomerTier?> GetTierAsync(int tierId)
        => _ctx.CustomerTiers.FirstOrDefaultAsync(t => t.ItemID == tierId);

    public Task<CustomerTier?> GetTierByLevelAsync(CustomerTierLevel level)
        => _ctx.CustomerTiers.FirstOrDefaultAsync(t => t.TierLevel == level && t.IsActive);

    public async Task<CustomerTier> UpsertTierAsync(CustomerTier tier)
    {
        var existing = await _ctx.CustomerTiers.FirstOrDefaultAsync(t => t.TierLevel == tier.TierLevel);
        if (existing == null)
        {
            _ctx.CustomerTiers.Add(tier);
        }
        else
        {
            existing.TierName = tier.TierName;
            existing.MinPoints = tier.MinPoints;
            existing.MaxPoints = tier.MaxPoints;
            existing.DiscountPercent = tier.DiscountPercent;
            existing.PointsPerDollar = tier.PointsPerDollar;
            existing.BadgeColor = tier.BadgeColor;
            existing.Description = tier.Description;
            existing.IsActive = tier.IsActive;
        }
        await _ctx.SaveChangesAsync();
        return existing ?? tier;
    }

    // ── Stats ─────────────────────────────────────────────────────────────

    public Task<int> CountCustomersAsync()
        => _ctx.CustomerProfiles.CountAsync();

    public Task<int> CountNewCustomersAsync(int days)
    {
        var cutoff = DateTimeOffset.UtcNow.AddDays(-days);
        return _ctx.CustomerProfiles.CountAsync(p => p.CreatedAt >= cutoff);
    }

    public async Task<int> CountActiveCustomersAsync(int days)
    {
        var cutoff = DateTimeOffset.UtcNow.AddDays(-days);
        return await _ctx.Orders
            .Where(o => o.CreatedAt >= cutoff && o.CustomerId.HasValue)
            .Select(o => o.CustomerId!.Value)
            .Distinct()
            .CountAsync();
    }

    public async Task<Dictionary<CustomerTierLevel, int>> GetTierDistributionAsync()
    {
        var rows = await _ctx.CustomerProfiles
            .GroupBy(p => p.TierLevel)
            .Select(g => new { Level = g.Key, Count = g.Count() })
            .ToListAsync();
        return rows.ToDictionary(r => r.Level, r => r.Count);
    }

    // ── User helpers ───────────────────────────────────────────────────────

    public async Task UpdateUserNameAsync(int userId, string fullName)
    {
        var user = await _ctx.Users.FindAsync(userId);
        if (user == null) return;

        var parts = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        user.FirstName = parts.Length >= 1 ? parts[0] : user.FirstName;
        user.LastName   = parts.Length >= 2 ? parts[^1] : string.Empty;
        user.MiddleName = parts.Length >= 3 ? string.Join(" ", parts[1..^1]) : null;

        await _ctx.SaveChangesAsync();
    }
}
