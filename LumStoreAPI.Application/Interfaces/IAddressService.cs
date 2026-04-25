using LumStoreAPI.Application.DTOs.AddressDTO;
using LumStoreAPI.Application.DTOs.Responses;

namespace LumStoreAPI.Application.Interfaces;

public interface IAddressService
{
    Task<APIResponse<IEnumerable<AddressDTO>>> GetAddressesAsync(CancellationToken ct = default);
    Task<APIResponse<AddressDTO>> AddAddressAsync(AddressRequest request, CancellationToken ct = default);
    Task<APIResponse<AddressDTO>> UpdateAddressAsync(int id, AddressRequest request, CancellationToken ct = default);
    Task<APIResponseBase> DeleteAddressAsync(int id, CancellationToken ct = default);
    Task<APIResponseBase> SetDefaultAsync(int id, CancellationToken ct = default);
}
