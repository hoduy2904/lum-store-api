using LumStoreAPI.Core.Entities.Customers;

namespace LumStoreAPI.Infrastructure.Repositories.Interfaces;

public interface ICustomerAddressRepository
{
    Task<IEnumerable<CustomerAddress>> GetByUserIdAsync(int userId, CancellationToken ct = default);
    Task<CustomerAddress?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<CustomerAddress> InsertAsync(CustomerAddress address, CancellationToken ct = default);
    Task<CustomerAddress> UpdateAsync(int id, Action<CustomerAddress> update, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);

    /// <summary>
    /// Sets <paramref name="addressId"/> as default and clears all other addresses for
    /// the user in a single database transaction.
    /// </summary>
    Task SetDefaultAsync(int userId, int addressId, CancellationToken ct = default);

    /// <summary>
    /// Clears IsDefault on every address belonging to <paramref name="userId"/> except
    /// <paramref name="excludeId"/>. Pass <c>null</c> to clear all without exclusion.
    /// </summary>
    Task ClearDefaultsAsync(int userId, int? excludeId = null, CancellationToken ct = default);

    Task<bool> ExistsForUserAsync(int id, int userId, CancellationToken ct = default);
    Task<CustomerAddress?> GetByUserAsync(int id, int userId, CancellationToken ct = default);
}
