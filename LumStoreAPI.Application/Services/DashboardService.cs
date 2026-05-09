using LumStoreAPI.Application.DTOs.DashboardDTO;
using LumStoreAPI.Application.Interfaces;
using LumStoreAPI.Core.Models.Enums;
using LumStoreAPI.Infrastructure;
using LumStoreAPI.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

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

        var ordersByStatusTask = _orderRepo.CountOrdersByStatusAsync();
        var totalRevenueTask = _orderRepo.SumRevenueAsync();
        var revenueTodayTask = _orderRepo.SumRevenueAsync(todayStart, todayStart.AddDays(1));
        var totalProductsTask = _ctx.Products.CountAsync();
        var customerStatsTask = _customerRepo.GetTierDistributionAsync();
        var totalCustomersTask = _customerRepo.CountCustomersAsync();

        await Task.WhenAll(ordersByStatusTask, totalRevenueTask, revenueTodayTask,
            totalProductsTask, customerStatsTask, totalCustomersTask);

        var ordersByStatus = ordersByStatusTask.Result;
        var customerStats = customerStatsTask.Result;

        return new DashboardStatsDTO
        {
            TotalOrders = ordersByStatus.Values.Sum(),
            PendingOrders = ordersByStatus.GetValueOrDefault(OrderStatus.Pending),
            ProcessingOrders = ordersByStatus.GetValueOrDefault(OrderStatus.Processing),
            TotalRevenue = totalRevenueTask.Result,
            RevenueToday = revenueTodayTask.Result,
            TotalProducts = totalProductsTask.Result,
            TotalCustomers = totalCustomersTask.Result,
            VipCustomers = customerStats.GetValueOrDefault(CustomerTierLevel.VIP),
            OrdersByStatus = ordersByStatus.ToDictionary(k => k.Key.ToString(), v => v.Value),
            CustomersByTier = customerStats.ToDictionary(k => k.Key.ToString(), v => v.Value)
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
                new DateTimeOffset(now.Date.AddDays(-(((int)now.DayOfWeek + 6) % 7)), TimeSpan.Zero),
                new DateTimeOffset(now.Date.AddDays(7 - ((int)now.DayOfWeek + 6) % 7), TimeSpan.Zero),
                new DateTimeOffset(now.Date.AddDays(-7 - ((int)now.DayOfWeek + 6) % 7), TimeSpan.Zero),
                new DateTimeOffset(now.Date.AddDays(-(((int)now.DayOfWeek + 6) % 7)), TimeSpan.Zero)
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
