using LumStoreAPI.Application.DTOs.DashboardDTO;
using LumStoreAPI.Application.DTOs.Responses;
using LumStoreAPI.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LumStoreAPI.Controllers;

/// <summary>Dashboard statistics and charts data.</summary>
[Route("api/dashboard")]
[ApiController]
[Authorize(Roles = "ADMIN")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService) => _dashboardService = dashboardService;

    /// <summary>GET /api/dashboard/stats — Top-level KPI stats.</summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var stats = await _dashboardService.GetStatsAsync();
        return Ok(APIResponse<DashboardStatsDTO>.Success(stats));
    }

    /// <summary>GET /api/dashboard/daily?from=&amp;to= — Daily order/revenue chart data.</summary>
    [HttpGet("daily")]
    public async Task<IActionResult> GetDailyStats(
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to)
    {
        var now = DateTimeOffset.UtcNow;
        var fromDate = from ?? now.AddDays(-30);
        var toDate = to ?? now;

        var stats = await _dashboardService.GetDailyStatsAsync(fromDate, toDate);
        return Ok(APIResponse<IEnumerable<DailyStatDTO>>.Success(stats));
    }

    /// <summary>GET /api/dashboard/revenue?period=day|week|month — Revenue widget.</summary>
    [HttpGet("revenue")]
    public async Task<IActionResult> GetRevenueWidget([FromQuery] string period = "month")
    {
        if (!new[] { "day", "week", "month" }.Contains(period))
            return BadRequest(APIResponseBase.Failure("INVALID_PARAM", ["period must be day, week, or month"]));

        var widget = await _dashboardService.GetRevenueWidgetAsync(period);
        return Ok(APIResponse<RevenueWidgetDTO>.Success(widget));
    }
}
