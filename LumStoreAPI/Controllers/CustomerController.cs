using LumStoreAPI.Application.DTOs.CustomerDTO;
using LumStoreAPI.Application.DTOs.Responses;
using LumStoreAPI.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LumStoreAPI.Controllers;

/// <summary>Customer management — profiles, loyalty points, tiers, notes.</summary>
[Route("api/customers")]
[ApiController]
[Authorize(Roles = "ADMIN")]
public class CustomerController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomerController(ICustomerService customerService) => _customerService = customerService;

    // ── Profiles ──────────────────────────────────────────────────────────

    /// <summary>GET /api/customers — Paginated customer list.</summary>
    [HttpGet]
    public async Task<IActionResult> GetCustomers([FromQuery] CustomerListRequest request)
    {
        var result = await _customerService.GetCustomersAsync(request);
        return Ok(result);
    }

    /// <summary>GET /api/customers/{profileId}</summary>
    [HttpGet("{profileId:int}")]
    public async Task<IActionResult> GetCustomer(int profileId)
    {
        var customer = await _customerService.GetCustomerAsync(profileId);
        if (customer == null) return NotFound(APIResponseBase.Failure("NOT_FOUND", ["Customer not found"]));
        return Ok(APIResponse<CustomerGetDTO>.Success(customer));
    }

    /// <summary>GET /api/customers/by-user/{userId}</summary>
    [HttpGet("by-user/{userId:int}")]
    public async Task<IActionResult> GetCustomerByUserId(int userId)
    {
        var customer = await _customerService.GetCustomerByUserIdAsync(userId);
        if (customer == null) return NotFound(APIResponseBase.Failure("NOT_FOUND", ["Customer profile not found"]));
        return Ok(APIResponse<CustomerGetDTO>.Success(customer));
    }

    /// <summary>PUT /api/customers/{profileId} — Update customer profile.</summary>
    [HttpPut("{profileId:int}")]
    public async Task<IActionResult> UpdateProfile(int profileId, [FromBody] CustomerUpdateDTO dto)
    {
        var updated = await _customerService.UpdateProfileAsync(profileId, dto);
        return Ok(APIResponse<CustomerGetDTO>.Success(updated, ["Profile updated"]));
    }

    // ── Stats ─────────────────────────────────────────────────────────────

    /// <summary>GET /api/customers/stats</summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var stats = await _customerService.GetStatsAsync();
        return Ok(APIResponse<CustomerStatsDTO>.Success(stats));
    }

    // ── Notes ─────────────────────────────────────────────────────────────

    /// <summary>GET /api/customers/{profileId}/notes</summary>
    [HttpGet("{profileId:int}/notes")]
    public async Task<IActionResult> GetNotes(int profileId)
    {
        var notes = await _customerService.GetCustomerNotesAsync(profileId);
        return Ok(APIResponse<IEnumerable<CustomerNoteGetDTO>>.Success(notes));
    }

    /// <summary>POST /api/customers/{profileId}/notes</summary>
    [HttpPost("{profileId:int}/notes")]
    public async Task<IActionResult> AddNote(int profileId, [FromBody] CustomerNoteCreateDTO dto)
    {
        var (userId, userName) = GetCurrentUserInfo();
        var note = await _customerService.AddCustomerNoteAsync(profileId, dto, userId ?? 0, userName);
        return Ok(APIResponse<CustomerNoteGetDTO>.Success(note, ["Note added"]));
    }

    /// <summary>DELETE /api/customers/notes/{noteId}</summary>
    [HttpDelete("notes/{noteId:int}")]
    public async Task<IActionResult> DeleteNote(int noteId)
    {
        var deleted = await _customerService.DeleteCustomerNoteAsync(noteId);
        if (!deleted) return NotFound(APIResponseBase.Failure("NOT_FOUND", ["Note not found"]));
        return Ok(APIResponseBase.Success(["Note deleted"]));
    }

    // ── Loyalty Points ────────────────────────────────────────────────────

    /// <summary>GET /api/customers/{profileId}/points</summary>
    [HttpGet("{profileId:int}/points")]
    public async Task<IActionResult> GetLoyaltyPoints(int profileId)
    {
        var points = await _customerService.GetLoyaltyPointsAsync(profileId);
        return Ok(APIResponse<IEnumerable<LoyaltyPointGetDTO>>.Success(points));
    }

    /// <summary>POST /api/customers/{profileId}/points/award</summary>
    [HttpPost("{profileId:int}/points/award")]
    public async Task<IActionResult> AwardPoints(int profileId, [FromBody] AwardPointsDTO dto)
    {
        await _customerService.AwardPointsAsync(profileId, dto.Points, dto.Description);
        return Ok(APIResponseBase.Success([$"{dto.Points} points awarded"]));
    }

    /// <summary>POST /api/customers/{profileId}/points/redeem</summary>
    [HttpPost("{profileId:int}/points/redeem")]
    public async Task<IActionResult> RedeemPoints(int profileId, [FromBody] AwardPointsDTO dto)
    {
        await _customerService.RedeemPointsAsync(profileId, dto.Points, dto.Description);
        return Ok(APIResponseBase.Success([$"{dto.Points} points redeemed"]));
    }

    // ── Tiers ─────────────────────────────────────────────────────────────

    /// <summary>GET /api/customers/tiers</summary>
    [HttpGet("tiers")]
    public async Task<IActionResult> GetTiers()
    {
        var tiers = await _customerService.GetTiersAsync();
        return Ok(APIResponse<IEnumerable<CustomerTierGetDTO>>.Success(tiers));
    }

    /// <summary>PUT /api/customers/tiers — Upsert a tier configuration.</summary>
    [HttpPut("tiers")]
    public async Task<IActionResult> UpsertTier([FromBody] CustomerTierUpsertDTO dto)
    {
        var tier = await _customerService.UpsertTierAsync(dto);
        return Ok(APIResponse<CustomerTierGetDTO>.Success(tier, ["Tier saved"]));
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private (int? id, string name) GetCurrentUserInfo()
    {
        int.TryParse(User.Claims.FirstOrDefault(c => c.Type == "id")?.Value, out int id);
        var name = User.Claims.FirstOrDefault(c => c.Type == "name")?.Value ?? "Unknown";
        return (id > 0 ? id : null, name);
    }
}
