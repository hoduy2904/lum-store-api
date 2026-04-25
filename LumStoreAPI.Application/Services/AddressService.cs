using LumStoreAPI.Application.DTOs.AddressDTO;
using LumStoreAPI.Application.DTOs.Responses;
using LumStoreAPI.Application.Interfaces;
using LumStoreAPI.Core.Entities.Customers;
using LumStoreAPI.Infrastructure.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;

namespace LumStoreAPI.Application.Services;

internal class AddressService : IAddressService
{
    private readonly ICustomerAddressRepository _repo;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AddressService(ICustomerAddressRepository repo, IHttpContextAccessor httpContextAccessor)
    {
        _repo = repo;
        _httpContextAccessor = httpContextAccessor;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private int GetCurrentUserId()
    {
        var claim = _httpContextAccessor.HttpContext?.User
            .Claims.FirstOrDefault(x => x.Type == "id")?.Value;

        if (!int.TryParse(claim, out int userId))
            throw new UnauthorizedAccessException("User identity could not be resolved.");

        return userId;
    }

    // ── Queries ───────────────────────────────────────────────────────────────

    public async Task<APIResponse<IEnumerable<AddressDTO>>> GetAddressesAsync(CancellationToken ct = default)
    {
        var userId = GetCurrentUserId();
        var addresses = await _repo.GetByUserIdAsync(userId, ct);
        return APIResponse<IEnumerable<AddressDTO>>.Success(addresses.Select(a => new AddressDTO(a)));
    }

    // ── Commands ──────────────────────────────────────────────────────────────

    public async Task<APIResponse<AddressDTO>> AddAddressAsync(AddressRequest request, CancellationToken ct = default)
    {
        var userId = GetCurrentUserId();

        // Insert first so we have the new ID, then clear all *other* defaults.
        // This avoids a window where the user has no default address.
        var address = new CustomerAddress
        {
            UserId = userId,
            Phone = request.Phone,
            Address = request.Address,
            City = request.City,
            State = request.State,
            Details = request.Details,
            IsDefault = request.IsDefault,
        };

        var created = await _repo.InsertAsync(address, ct);

        if (request.IsDefault)
            await _repo.ClearDefaultsAsync(userId, excludeId: created.ItemID, ct);

        return APIResponse<AddressDTO>.Success(new AddressDTO(created));
    }

    public async Task<APIResponse<AddressDTO>> UpdateAddressAsync(int id, AddressRequest request, CancellationToken ct = default)
    {
        var userId = GetCurrentUserId();

        if (!await _repo.ExistsForUserAsync(id, userId, ct))
            return APIResponse<AddressDTO>.Failure("Forbidden");

        var updated = await _repo.UpdateAsync(id, a =>
        {
            a.Phone = request.Phone;
            a.Address = request.Address;
            a.City = request.City;
            a.State = request.State;
            a.Details = request.Details;
            a.IsDefault = request.IsDefault;
        }, ct);

        // If promoted to default, demote all others.
        if (request.IsDefault)
            await _repo.ClearDefaultsAsync(userId, excludeId: id, ct);

        return APIResponse<AddressDTO>.Success(new AddressDTO(updated));
    }

    public async Task<APIResponseBase> DeleteAddressAsync(int id, CancellationToken ct = default)
    {
        var userId = GetCurrentUserId();

        var address = await _repo.GetByIdAsync(id, ct);
        if (address is null || address.UserId != userId)
            return APIResponseBase.Failure("Forbidden");

        if (address.IsDefault)
            return APIResponseBase.Failure("Cannot delete the default address. Set another address as default first.");

        await _repo.DeleteAsync(id, ct);
        return APIResponseBase.Success();
    }

    public async Task<APIResponseBase> SetDefaultAsync(int id, CancellationToken ct = default)
    {
        var userId = GetCurrentUserId();

        if (!await _repo.ExistsForUserAsync(id, userId, ct))
            return APIResponseBase.Failure("Address not found.");

        // Atomic: clears all + sets target in one transaction (see repository).
        await _repo.SetDefaultAsync(userId, id, ct);
        return APIResponseBase.Success();
    }
}
