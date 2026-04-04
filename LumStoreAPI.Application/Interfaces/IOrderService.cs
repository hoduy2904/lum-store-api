using LumStoreAPI.Application.DTOs.OrderDTO;
using LumStoreAPI.Application.DTOs.Responses;
using LumStoreAPI.Core.Interfaces.ContentEngine;
using LumStoreAPI.Core.Models.Enums;

namespace LumStoreAPI.Application.Interfaces;

public interface IOrderService
{
    Task<PagedResponse<OrderGetDTO>> GetOrdersAsync(OrderListRequest request);
    Task<OrderGetDTO?> GetOrderAsync(int orderId);
    Task<OrderGetDTO?> GetOrderByCodeAsync(string orderCode);
    Task<OrderGetDTO> CreateOrderAsync(OrderCreateDTO dto, int? operatorUserId = null);
    Task<OrderGetDTO> UpdateOrderStatusAsync(int orderId, OrderUpdateStatusDTO dto, int? operatorUserId = null);
    Task<bool> DeleteOrderAsync(int orderId);

    // ── History ───────────────────────────────────────────────────────────
    Task<IEnumerable<OrderHistoryGetDTO>> GetOrderHistoriesAsync(int orderId);

    // ── Notes ─────────────────────────────────────────────────────────────
    Task<IEnumerable<OrderNoteGetDTO>> GetOrderNotesAsync(int orderId);
    Task<OrderNoteGetDTO> AddOrderNoteAsync(int orderId, OrderNoteCreateDTO dto, int authorId, string authorName);
    Task<bool> DeleteOrderNoteAsync(int noteId);

    // ── Returns ───────────────────────────────────────────────────────────
    Task<IEnumerable<OrderReturnGetDTO>> GetOrderReturnsAsync(int orderId);
    Task<OrderReturnGetDTO?> GetOrderReturnAsync(int returnId);
    Task<OrderReturnGetDTO> CreateReturnAsync(int orderId, OrderReturnCreateDTO dto);
    Task<OrderReturnGetDTO> ReviewReturnAsync(int returnId, OrderReturnReviewDTO dto, int reviewerId, string reviewerName);
}
