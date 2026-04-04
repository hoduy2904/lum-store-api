using LumStoreAPI.Core.Models.Enums;

namespace LumStoreAPI.Application.DTOs.DashboardDTO;

public class DashboardStatsDTO
{
    public int TotalOrders { get; set; }
    public int PendingOrders { get; set; }
    public int ProcessingOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal RevenueToday { get; set; }
    public int TotalProducts { get; set; }
    public int TotalCustomers { get; set; }
    public int VipCustomers { get; set; }
    public Dictionary<string, int> OrdersByStatus { get; set; } = [];
    public Dictionary<string, int> CustomersByTier { get; set; } = [];
}

public class DailyStatDTO
{
    public DateTimeOffset Date { get; set; }
    public int OrderCount { get; set; }
    public decimal Revenue { get; set; }
}

public class RevenueWidgetDTO
{
    public string Period { get; set; } = default!;   // day | week | month
    public decimal Current { get; set; }
    public decimal Previous { get; set; }
    public decimal ChangePercent => Previous == 0 ? 100 : Math.Round((Current - Previous) / Previous * 100, 2);
    public IEnumerable<DailyStatDTO> Breakdown { get; set; } = [];
}
