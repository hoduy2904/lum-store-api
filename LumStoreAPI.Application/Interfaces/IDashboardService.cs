using LumStoreAPI.Application.DTOs.DashboardDTO;

namespace LumStoreAPI.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardStatsDTO> GetStatsAsync();
    Task<IEnumerable<DailyStatDTO>> GetDailyStatsAsync(DateTimeOffset from, DateTimeOffset to);
    Task<RevenueWidgetDTO> GetRevenueWidgetAsync(string period);  // "day" | "week" | "month"
}
