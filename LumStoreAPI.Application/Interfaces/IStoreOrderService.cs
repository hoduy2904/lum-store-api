using LumStoreAPI.Application.DTOs.OrderDTO;
using LumStoreAPI.Application.DTOs.Responses;
using LumStoreAPI.Application.DTOs.StoreOrderDTO;

namespace LumStoreAPI.Application.Interfaces;

public interface IStoreOrderService
{
    Task<APIResponse<StoreOrderSummaryDTO>> PlaceOrderAsync(StorePlaceOrderRequest request, CancellationToken ct = default);
    Task<PagedResponse<StoreOrderSummaryDTO>> GetOrdersAsync(int page, int pageSize, CancellationToken ct = default);
    Task<APIResponse<StoreOrderDetailDTO>> GetOrderAsync(int orderId, CancellationToken ct = default);

    // ── Checkout Preview ──────────────────────────────────────────────────
    Task<APIResponse<StoreCheckoutPreviewDTO>> GetCheckoutPreviewAsync(StoreCheckoutPreviewRequest request, CancellationToken ct = default);

    // ── Cancel ────────────────────────────────────────────────────────────
    Task<APIResponse<bool>> CancelOrderAsync(int orderId, StoreCancelOrderRequest request, CancellationToken ct = default);

    // ── Returns ───────────────────────────────────────────────────────────
    Task<APIResponse<IEnumerable<OrderReturnGetDTO>>> GetOrderReturnsAsync(int orderId, CancellationToken ct = default);
    Task<APIResponse<OrderReturnGetDTO>> SubmitReturnAsync(int orderId, OrderReturnCreateDTO dto, CancellationToken ct = default);
}
