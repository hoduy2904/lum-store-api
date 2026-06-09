using LumStoreAPI.Application.DTOs.OrderDTO;
using LumStoreAPI.Application.DTOs.Responses;
using LumStoreAPI.Application.DTOs.ShiprelayDTO;
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
    Task<OrderGetDTO> UpdateOrderTrackingAsync(int orderId, OrderUpdateTrackingDTO dto, int? operatorUserId = null);
    Task<ShiprelayTrackingResult?> GetOrderTrackingAsync(int orderId);
    Task<OrderGetDTO?> SyncOrderFromShiprelayAsync(int orderId);
    /// <summary>Bulk-update orders from a list of ShipRelay shipment summaries (manual/scheduled sync).
    /// Looks up each order by OrderRef (= OrderCode), applies status/tracking changes, writes OrderHistory.
    /// Returns (synced, failed) counts.</summary>
    Task<(int Synced, int Failed)> BulkUpdateFromShipmentsAsync(IEnumerable<ShiprelayShipmentSummaryDTO> shipments);
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
    Task<PagedResponse<OrderReturnGetDTO>> GetAllReturnsAsync(int page, int pageSize, ReturnStatus? status, string? search);
    Task<OrderReturnGetDTO> CreateReturnAsync(int orderId, OrderReturnCreateDTO dto);
    Task<OrderReturnGetDTO> ReviewReturnAsync(int returnId, OrderReturnReviewDTO dto, int reviewerId, string reviewerName);
}
