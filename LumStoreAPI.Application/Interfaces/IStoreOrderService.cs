using LumStoreAPI.Application.DTOs.Responses;
using LumStoreAPI.Application.DTOs.StoreOrderDTO;

namespace LumStoreAPI.Application.Interfaces;

public interface IStoreOrderService
{
    Task<APIResponse<StoreOrderSummaryDTO>> PlaceOrderAsync(StorePlaceOrderRequest request, CancellationToken ct = default);
    Task<PagedResponse<StoreOrderSummaryDTO>> GetOrdersAsync(int page, int pageSize, CancellationToken ct = default);
    Task<APIResponse<StoreOrderDetailDTO>> GetOrderAsync(int orderId, CancellationToken ct = default);
}
