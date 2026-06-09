using LumStoreAPI.Core.Entities.Orders;
using LumStoreAPI.Core.Interfaces.ContentEngine;
using LumStoreAPI.Core.Models.Enums;
using LumStoreAPI.Infrastructure.Repositories.Interfaces;
using LumStoreAPI.Infrastructure.Types;
using Microsoft.EntityFrameworkCore;

namespace LumStoreAPI.Infrastructure.Repositories.Presentations;

internal class OrderRepository : IOrderRepository
{
    private readonly LumStoreContext _ctx;
    public OrderRepository(LumStoreContext ctx) => _ctx = ctx;

    public Task<Order?> GetOrderAsync(int orderId)
        => _ctx.Orders
               .Include(o => o.OrderItems)
               .FirstOrDefaultAsync(o => o.ItemID == orderId);

    public Task<Order?> GetOrderByCodeAsync(string orderCode)
        => _ctx.Orders
               .Include(o => o.OrderItems)
               .FirstOrDefaultAsync(o => o.OrderCode == orderCode);

    public async Task<IPagedEnumerable<Order>> GetOrdersAsync(
        int page, int pageSize,
        OrderStatus? status = null,
        PaymentStatus? paymentStatus = null,
        string? search = null,
        int? customerId = null,
        DateTimeOffset? fromDate = null,
        DateTimeOffset? toDate = null,
        string sortBy = "CreatedAt",
        bool descending = true)
    {
        IQueryable<Order> query = _ctx.Orders;

        if (status.HasValue) query = query.Where(o => o.Status == status.Value);
        if (paymentStatus.HasValue) query = query.Where(o => o.PaymentStatus == paymentStatus.Value);
        if (customerId.HasValue) query = query.Where(o => o.CustomerId == customerId.Value);
        if (fromDate.HasValue) query = query.Where(o => o.CreatedAt >= fromDate.Value);
        if (toDate.HasValue) query = query.Where(o => o.CreatedAt <= toDate.Value);
        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim().ToLower();
            query = query.Where(o =>
                o.OrderCode.ToLower().Contains(search) ||
                o.CustomerName.ToLower().Contains(search) ||
                o.CustomerEmail.ToLower().Contains(search));
        }

        query = sortBy switch
        {
            "Total"       => descending ? query.OrderByDescending(o => o.Total) : query.OrderBy(o => o.Total),
            "Status"      => descending ? query.OrderByDescending(o => o.Status) : query.OrderBy(o => o.Status),
            "CustomerName"=> descending ? query.OrderByDescending(o => o.CustomerName) : query.OrderBy(o => o.CustomerName),
            _             => descending ? query.OrderByDescending(o => o.CreatedAt) : query.OrderBy(o => o.CreatedAt)
        };

        int total = await query.CountAsync();
        var data = await query.Skip((page - 1) * pageSize).Take(pageSize)
            .Include(o => o.OrderItems)
            .AsNoTracking()
            .ToArrayAsync();
        return new PagedEnumerable<Order>(data, total);
    }

    public async Task<Order> InsertOrderAsync(Order order)
    {
        _ctx.Orders.Add(order);
        await _ctx.SaveChangesAsync();
        return order;
    }

    public async Task<Order> UpdateOrderAsync(int orderId, Action<Order> update)
    {
        var order = await _ctx.Orders.FindAsync(orderId)
            ?? throw new KeyNotFoundException($"Order {orderId} not found");
        update(order);
        await _ctx.SaveChangesAsync();
        return order;
    }

    public async Task<Order> UpdateOrderAsync(Order order, Action<Order> update, OrderHistory? history = null)
    {
        update(order);
        if (history is not null)
            _ctx.OrderHistories.Add(history);
        await _ctx.SaveChangesAsync();
        return order;
    }

    public async Task<bool> DeleteOrderAsync(int orderId)
    {
        var order = await _ctx.Orders.FindAsync(orderId);
        if (order == null) return false;
        _ctx.Orders.Remove(order);
        await _ctx.SaveChangesAsync();
        return true;
    }

    // ── Backfill ──────────────────────────────────────────────────────────

    public Task<List<Order>> GetOrdersMissingRateFieldsAsync(CancellationToken ct = default)
        => _ctx.Orders
               .AsNoTracking()
               .Where(o => o.ShiprelayShipmentId != null && o.ShiprelayShipmentId != "" && o.ShippingServiceName == null)
               .Select(o => new Order { ItemID = o.ItemID, OrderCode = o.OrderCode, ShiprelayShipmentId = o.ShiprelayShipmentId })
               .ToListAsync(ct);

    // ── History ───────────────────────────────────────────────────────────

    public async Task<IEnumerable<OrderHistory>> GetOrderHistoriesAsync(int orderId)
        => await _ctx.OrderHistories
                .AsNoTracking()
                .Where(h => h.OrderId == orderId)
                .OrderByDescending(h => h.CreatedAt)
                .ToListAsync();

    public async Task<OrderHistory> InsertOrderHistoryAsync(OrderHistory history)
    {
        _ctx.OrderHistories.Add(history);
        await _ctx.SaveChangesAsync();
        return history;
    }

    // ── Notes ─────────────────────────────────────────────────────────────

    public async Task<IEnumerable<OrderNote>> GetOrderNotesAsync(int orderId)
        => await _ctx.OrderNotes
                .AsNoTracking()
                .Where(n => n.OrderId == orderId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

    public async Task<OrderNote> InsertOrderNoteAsync(OrderNote note)
    {
        _ctx.OrderNotes.Add(note);
        await _ctx.SaveChangesAsync();
        return note;
    }

    public async Task<bool> DeleteOrderNoteAsync(int noteId)
    {
        var note = await _ctx.OrderNotes.FindAsync(noteId);
        if (note == null) return false;
        _ctx.OrderNotes.Remove(note);
        await _ctx.SaveChangesAsync();
        return true;
    }

    // ── Returns ───────────────────────────────────────────────────────────

    public async Task<IEnumerable<OrderReturn>> GetOrderReturnsAsync(int orderId)
        => await _ctx.OrderReturns
                .AsNoTracking()
                .Include(r => r.Order)
                .Include(r => r.ReviewedBy)
                .Include(r => r.ReturnItems)
                    .ThenInclude(ri => ri.OrderItem)
                .Where(r => r.OrderId == orderId)
                .ToListAsync();

    public Task<OrderReturn?> GetOrderReturnAsync(int returnId)
        => _ctx.OrderReturns
               .Include(r => r.Order)
               .Include(r => r.ReviewedBy)
               .Include(r => r.ReturnItems)
                   .ThenInclude(ri => ri.OrderItem)
               .FirstOrDefaultAsync(r => r.ItemID == returnId);

    public async Task<IPagedEnumerable<OrderReturn>> GetAllReturnsAsync(int page, int pageSize, ReturnStatus? status, string? search)
    {
        IQueryable<OrderReturn> query = _ctx.OrderReturns
            .AsNoTracking()
            .Include(r => r.Order)
            .Include(r => r.ReviewedBy)
            .Include(r => r.ReturnItems)
                .ThenInclude(ri => ri.OrderItem);

        if (status.HasValue) query = query.Where(r => r.Status == status.Value);
        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim().ToLower();
            query = query.Where(r =>
                r.Order.OrderCode.ToLower().Contains(search) ||
                r.Order.CustomerName.ToLower().Contains(search) ||
                r.Order.CustomerEmail.ToLower().Contains(search));
        }

        query = query.OrderByDescending(r => r.CreatedAt);

        int total = await query.CountAsync();
        var data = await query.Skip((page - 1) * pageSize).Take(pageSize).ToArrayAsync();
        return new PagedEnumerable<OrderReturn>(data, total);
    }

    public async Task<OrderReturn> InsertOrderReturnAsync(OrderReturn orderReturn)
    {
        _ctx.OrderReturns.Add(orderReturn);
        await _ctx.SaveChangesAsync();
        return orderReturn;
    }

    public async Task<OrderReturn> UpdateOrderReturnAsync(int returnId, Action<OrderReturn> update)
    {
        var ret = await _ctx.OrderReturns.FindAsync(returnId)
            ?? throw new KeyNotFoundException($"OrderReturn {returnId} not found");
        update(ret);
        await _ctx.SaveChangesAsync();
        return ret;
    }

    // ── Stats ─────────────────────────────────────────────────────────────

    public async Task<Dictionary<OrderStatus, int>> CountOrdersByStatusAsync()
    {
        var rows = await _ctx.Orders
            .GroupBy(o => o.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        return rows.ToDictionary(r => r.Status, r => r.Count);
    }

    public Task<int> CountOrdersAsync(OrderStatus? status = null, DateTimeOffset? fromDate = null, DateTimeOffset? toDate = null)
    {
        IQueryable<Order> q = _ctx.Orders;
        if (status.HasValue) q = q.Where(o => o.Status == status.Value);
        if (fromDate.HasValue) q = q.Where(o => o.CreatedAt >= fromDate.Value);
        if (toDate.HasValue) q = q.Where(o => o.CreatedAt <= toDate.Value);
        return q.CountAsync();
    }

    public async Task<decimal> SumRevenueAsync(DateTimeOffset? fromDate = null, DateTimeOffset? toDate = null)
    {
        IQueryable<Order> q = _ctx.Orders.Where(o => o.PaymentStatus == PaymentStatus.Paid);
        if (fromDate.HasValue) q = q.Where(o => o.CreatedAt >= fromDate.Value);
        if (toDate.HasValue) q = q.Where(o => o.CreatedAt <= toDate.Value);
        return await q.SumAsync(o => (decimal?)o.Total) ?? 0m;
    }

    public async Task<IEnumerable<(DateTimeOffset Date, int Count, decimal Revenue)>> GetDailyStatsAsync(DateTimeOffset fromDate, DateTimeOffset toDate)
    {
        var rows = await _ctx.Orders
            .Where(o => o.CreatedAt >= fromDate && o.CreatedAt <= toDate)
            .GroupBy(o => o.CreatedAt.Date)
            .Select(g => new
            {
                Date = g.Key,
                Count = g.Count(),
                Revenue = g.Where(o => o.PaymentStatus == PaymentStatus.Paid).Sum(o => (decimal?)o.Total) ?? 0m
            })
            .OrderBy(x => x.Date)
            .ToListAsync();

        return rows.Select(r => (new DateTimeOffset(r.Date, TimeSpan.Zero), r.Count, r.Revenue));
    }
}
