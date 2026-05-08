using LumStoreAPI.Core.Entities.Orders;
using LumStoreAPI.Core.Interfaces.ContentEngine;
using LumStoreAPI.Core.Models.Enums;

namespace LumStoreAPI.Infrastructure.Repositories.Interfaces;

public interface IOrderRepository
{
    Task<Order?> GetOrderAsync(int orderId);
    Task<Order?> GetOrderByCodeAsync(string orderCode);
    Task<IPagedEnumerable<Order>> GetOrdersAsync(
        int page, int pageSize,
        OrderStatus? status = null,
        PaymentStatus? paymentStatus = null,
        string? search = null,
        int? customerId = null,
        DateTimeOffset? fromDate = null,
        DateTimeOffset? toDate = null,
        string sortBy = "CreatedAt",
        bool descending = true);
    Task<Order> InsertOrderAsync(Order order);
    Task<Order> UpdateOrderAsync(int orderId, Action<Order> update);
    Task<bool> DeleteOrderAsync(int orderId);

    // ── History ───────────────────────────────────────────────────────────
    Task<IEnumerable<OrderHistory>> GetOrderHistoriesAsync(int orderId);
    Task<OrderHistory> InsertOrderHistoryAsync(OrderHistory history);

    // ── Notes ─────────────────────────────────────────────────────────────
    Task<IEnumerable<OrderNote>> GetOrderNotesAsync(int orderId);
    Task<OrderNote> InsertOrderNoteAsync(OrderNote note);
    Task<bool> DeleteOrderNoteAsync(int noteId);

    // ── Returns ───────────────────────────────────────────────────────────
    Task<IEnumerable<OrderReturn>> GetOrderReturnsAsync(int orderId);
    Task<OrderReturn?> GetOrderReturnAsync(int returnId);
    Task<IPagedEnumerable<OrderReturn>> GetAllReturnsAsync(int page, int pageSize, ReturnStatus? status, string? search);
    Task<OrderReturn> InsertOrderReturnAsync(OrderReturn orderReturn);
    Task<OrderReturn> UpdateOrderReturnAsync(int returnId, Action<OrderReturn> update);

    // ── Stats ─────────────────────────────────────────────────────────────
    Task<int> CountOrdersAsync(OrderStatus? status = null, DateTimeOffset? fromDate = null, DateTimeOffset? toDate = null);
    Task<decimal> SumRevenueAsync(DateTimeOffset? fromDate = null, DateTimeOffset? toDate = null);
    Task<IEnumerable<(DateTimeOffset Date, int Count, decimal Revenue)>> GetDailyStatsAsync(DateTimeOffset fromDate, DateTimeOffset toDate);
}
