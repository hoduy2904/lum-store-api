using LumStoreAPI.Application.DTOs.DashboardDTO;
using LumStoreAPI.Application.Interfaces;
using LumStoreAPI.Core.Models.Enums;
using LumStoreAPI.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using LumStoreAPI.Infrastructure;

namespace LumStoreAPI.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IOrderRepository _orderRepo;
    private readonly ICustomerRepository _customerRepo;
    private readonly LumStoreContext _ctx;

    public DashboardService(IOrderRepository orderRepo, ICustomerRepository customerRepo, LumStoreContext ctx)
    {
        _orderRepo = orderRepo;
        _customerRepo = customerRepo;
        _ctx = ctx;
    }

    public async Task<DashboardStatsDTO> GetStatsAsync()
    {
        var today = DateTimeOffset.UtcNow.Date;
        var todayStart = new DateTimeOffset(today, TimeSpan.Zero);

        var totalOrders = await _orderRepo.CountOrdersAsync();
        var pendingOrders = await _orderRepo.CountOrdersAsync(OrderStatus.Pending);
        var processingOrders = await _orderRepo.CountOrdersAsync(OrderStatus.Processing);
        var totalRevenue = await _orderRepo.SumRevenueAsync();
        var revenueToday = await _orderRepo.SumRevenueAsync(todayStart, todayStart.AddDays(1));
        var totalProducts = await _ctx.Products.CountAsync();
        var customerStats = await _customerRepo.GetTierDistributionAsync();
        var totalCustomers = await _customerRepo.CountCustomersAsync();
        var vipCustomers = customerStats.GetValueOrDefault(CustomerTierLevel.VIP);

        // Order breakdown by status
        var ordersByStatus = new Dictionary<string, int>();
        foreach (OrderStatus status in Enum.GetValues<OrderStatus>())
        {
            var count = await _orderRepo.CountOrdersAsync(status);
            ordersByStatus[status.ToString()] = count;
        }

        return new DashboardStatsDTO
        {
            TotalOrders = totalOrders,
            PendingOrders = pendingOrders,
            ProcessingOrders = processingOrders,
            TotalRevenue = totalRevenue,
            RevenueToday = revenueToday,
            TotalProducts = totalProducts,
            TotalCustomers = totalCustomers,
            VipCustomers = vipCustomers,
            OrdersByStatus = ordersByStatus,
            CustomersByTier = customerStats.ToDictionary(
                k => k.Key.ToString(),
                v => v.Value)
        };
    }

    public async Task<IEnumerable<DailyStatDTO>> GetDailyStatsAsync(DateTimeOffset from, DateTimeOffset to)
    {
        var stats = await _orderRepo.GetDailyStatsAsync(from, to);
        return stats.Select(s => new DailyStatDTO
        {
            Date = s.Date,
            OrderCount = s.Count,
            Revenue = s.Revenue
        });
    }

    public async Task<RevenueWidgetDTO> GetRevenueWidgetAsync(string period)
    {
        var now = DateTimeOffset.UtcNow;

        var (currentFrom, currentTo, prevFrom, prevTo) = period switch
        {
            "day" => (
                new DateTimeOffset(now.Date, TimeSpan.Zero),
                new DateTimeOffset(now.Date.AddDays(1), TimeSpan.Zero),
                new DateTimeOffset(now.Date.AddDays(-1), TimeSpan.Zero),
                new DateTimeOffset(now.Date, TimeSpan.Zero)
            ),
            "week" => (
                new DateTimeOffset(now.Date.AddDays(-(int)now.DayOfWeek), TimeSpan.Zero),
                new DateTimeOffset(now.Date.AddDays(7 - (int)now.DayOfWeek), TimeSpan.Zero),
                new DateTimeOffset(now.Date.AddDays(-7 - (int)now.DayOfWeek), TimeSpan.Zero),
                new DateTimeOffset(now.Date.AddDays(-(int)now.DayOfWeek), TimeSpan.Zero)
            ),
            _ => (  // month
                new DateTimeOffset(new DateTime(now.Year, now.Month, 1), TimeSpan.Zero),
                new DateTimeOffset(new DateTime(now.Year, now.Month, 1).AddMonths(1), TimeSpan.Zero),
                new DateTimeOffset(new DateTime(now.Year, now.Month, 1).AddMonths(-1), TimeSpan.Zero),
                new DateTimeOffset(new DateTime(now.Year, now.Month, 1), TimeSpan.Zero)
            )
        };

        var current = await _orderRepo.SumRevenueAsync(currentFrom, currentTo);
        var previous = await _orderRepo.SumRevenueAsync(prevFrom, prevTo);
        var breakdown = await GetDailyStatsAsync(currentFrom, currentTo);

        return new RevenueWidgetDTO
        {
            Period = period,
            Current = current,
            Previous = previous,
            Breakdown = breakdown
        };
    }
}
