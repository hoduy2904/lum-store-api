using LumStoreAPI.Application.DTOs.AddressDTO;
using LumStoreAPI.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LumStoreAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class AddressController : ControllerBase
{
    private readonly IAddressService _addressService;

    public AddressController(IAddressService addressService)
    {
        _addressService = addressService;
    }

    /// <summary>GET /api/addresses — returns all addresses for the current user.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAddresses(CancellationToken ct)
    {
        var result = await _addressService.GetAddressesAsync(ct);
        return Ok(result);
    }

    /// <summary>POST /api/addresses — create a new address.</summary>
    [HttpPost]
    public async Task<IActionResult> AddAddress([FromBody] AddressRequest request, CancellationToken ct)
    {
        var result = await _addressService.AddAddressAsync(request, ct);
        return result.IsSuccess ? Created(string.Empty, result) : BadRequest(result);
    }

    /// <summary>PUT /api/addresses/{id} — update an existing address.</summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateAddress(int id, [FromBody] AddressRequest request, CancellationToken ct)
    {
        var result = await _addressService.UpdateAddressAsync(id, request, ct);
        return result.IsSuccess ? Ok(result) : NotFound(result);
    }

    /// <summary>DELETE /api/addresses/{id} — remove an address (default address is protected).</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteAddress(int id, CancellationToken ct)
    {
        var result = await _addressService.DeleteAddressAsync(id, ct);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>PATCH /api/addresses/{id}/set-default — promote one address to default.</summary>
    [HttpPatch("{id:int}/set-default")]
    public async Task<IActionResult> SetDefault(int id, CancellationToken ct)
    {
        var result = await _addressService.SetDefaultAsync(id, ct);
        return result.IsSuccess ? Ok(result) : NotFound(result);
    }
}
