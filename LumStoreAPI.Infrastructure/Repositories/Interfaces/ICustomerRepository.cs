using LumStoreAPI.Core.Entities.Customers;
using LumStoreAPI.Core.Interfaces.ContentEngine;
using LumStoreAPI.Core.Models.Enums;

namespace LumStoreAPI.Infrastructure.Repositories.Interfaces;

public interface ICustomerRepository
{
    Task<CustomerProfile?> GetProfileAsync(int profileId);
    Task<CustomerProfile?> GetProfileByUserIdAsync(int userId);
    Task<IPagedEnumerable<CustomerProfile>> GetProfilesAsync(
        int page, int pageSize,
        string? search = null,
        CustomerTierLevel? tierLevel = null,
        string sortBy = "CreatedAt",
        bool descending = true);
    Task<CustomerProfile> InsertProfileAsync(CustomerProfile profile);
    Task<CustomerProfile> UpdateProfileAsync(int profileId, Action<CustomerProfile> update);

    // ── Notes ─────────────────────────────────────────────────────────────
    Task<IEnumerable<CustomerNote>> GetCustomerNotesAsync(int profileId);
    Task<CustomerNote> InsertCustomerNoteAsync(CustomerNote note);
    Task<bool> DeleteCustomerNoteAsync(int noteId);

    // ── Loyalty Points ────────────────────────────────────────────────────
    Task<IEnumerable<LoyaltyPoint>> GetLoyaltyPointsAsync(int profileId);
    Task<LoyaltyPoint> InsertLoyaltyPointAsync(LoyaltyPoint point);

    // ── Tiers ─────────────────────────────────────────────────────────────
    Task<IEnumerable<CustomerTier>> GetTiersAsync();
    Task<CustomerTier?> GetTierAsync(int tierId);
    Task<CustomerTier?> GetTierByLevelAsync(CustomerTierLevel level);
    Task<CustomerTier> UpsertTierAsync(CustomerTier tier);

    // ── Stats ─────────────────────────────────────────────────────────────
    Task<int> CountCustomersAsync();
    Task<int> CountNewCustomersAsync(int days);
    Task<int> CountActiveCustomersAsync(int days);
    Task<Dictionary<CustomerTierLevel, int>> GetTierDistributionAsync();

    // ── User helpers ───────────────────────────────────────────────────────
    Task UpdateUserNameAsync(int userId, string fullName);
}
