using LumStoreAPI.Application.DTOs.CustomerDTO;
using LumStoreAPI.Application.DTOs.Responses;
using LumStoreAPI.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LumStoreAPI.ClientControllers;

[Route("api/store/profile")]
[ApiController]
[Authorize]
public class StoreProfileController(ICustomerService customerService) : ControllerBase
{
    private int CurrentUserId =>
        int.Parse(User.Claims.First(c => c.Type == "id").Value);

    /// <summary>GET /api/store/profile — Get own customer profile.</summary>
    [HttpGet]
    public async Task<IActionResult> GetProfile()
    {
        var profile = await customerService.GetOrCreateProfileAsync(CurrentUserId);
        return Ok(APIResponse<CustomerGetDTO>.Success(profile));
    }

    /// <summary>PUT /api/store/profile — Update own profile (phone, birthday).</summary>
    [HttpPut]
    public async Task<IActionResult> UpdateProfile([FromBody] CustomerUpdateDTO dto)
    {
        var profile = await customerService.GetOrCreateProfileAsync(CurrentUserId);
        var updated = await customerService.UpdateProfileAsync(profile.ProfileId, dto);
        return Ok(APIResponse<CustomerGetDTO>.Success(updated, ["Profile updated"]));
    }

    /// <summary>GET /api/store/profile/loyalty — Own loyalty balance and transaction history.</summary>
    [HttpGet("loyalty")]
    public async Task<IActionResult> GetLoyalty()
    {
        var profile = await customerService.GetOrCreateProfileAsync(CurrentUserId);
        var history = await customerService.GetLoyaltyPointsAsync(profile.ProfileId);

        var result = new StoreLoyaltyDTO
        {
            TotalPoints = profile.TotalPoints,
            AvailablePoints = profile.AvailablePoints,
            TierLevel = profile.TierName,
            History = history
        };

        return Ok(APIResponse<StoreLoyaltyDTO>.Success(result));
    }
}
