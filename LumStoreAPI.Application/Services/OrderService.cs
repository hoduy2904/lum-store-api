using LumStoreAPI.Application.DTOs.OrderDTO;
using LumStoreAPI.Application.DTOs.Responses;
using LumStoreAPI.Application.DTOs.ShiprelayDTO;
using LumStoreAPI.Application.Interfaces;
using LumStoreAPI.Core.Entities.Orders;
using LumStoreAPI.Core.Interfaces.ContentEngine;
using LumStoreAPI.Core.Interfaces.Sytems;
using LumStoreAPI.Core.Models.Enums;
using LumStoreAPI.Infrastructure.Extensions;
using LumStoreAPI.Infrastructure.Repositories.Interfaces;

namespace LumStoreAPI.Application.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepo;
    private readonly IEventLogService _eventLog;
    private readonly IShiprelayService _shiprelayService;

    public OrderService(IOrderRepository orderRepo, IEventLogService eventLog, IShiprelayService shiprelayService)
    {
        _orderRepo = orderRepo;
        _eventLog = eventLog;
        _shiprelayService = shiprelayService;
    }

    public async Task<PagedResponse<OrderGetDTO>> GetOrdersAsync(OrderListRequest request)
    {
        var paged = await _orderRepo.GetOrdersAsync(
            request.Page, request.PageSize,
            request.Status, request.PaymentStatus,
            request.Search, request.CustomerId,
            request.FromDate, request.ToDate,
            request.SortBy, request.Descending);

        var pagedDtos = paged.Select(MapToDTO).ToList().AsPagedEnumerable(paged.TotalRecords);
        return PagedResponse<OrderGetDTO>.Success(pagedDtos, request.Page, request.PageSize);
    }

    public async Task<OrderGetDTO?> GetOrderAsync(int orderId)
    {
        var order = await _orderRepo.GetOrderAsync(orderId);
        return order == null ? null : MapToDTO(order);
    }

    public async Task<OrderGetDTO?> GetOrderByCodeAsync(string orderCode)
    {
        var order = await _orderRepo.GetOrderByCodeAsync(orderCode);
        return order == null ? null : MapToDTO(order);
    }

    public async Task<OrderGetDTO> CreateOrderAsync(OrderCreateDTO dto, int? operatorUserId = null)
    {
        var order = new Order
        {
            OrderCode = GenerateOrderCode(),
            CustomerId = dto.CustomerId,
            CustomerName = dto.CustomerName,
            CustomerEmail = dto.CustomerEmail,
            CustomerPhone = dto.CustomerPhone,
            ShippingAddress = dto.ShippingAddress,
            ShippingCity = dto.ShippingCity,
            ShippingState = dto.ShippingState,
            ShippingZip = dto.ShippingZip,
            ShippingCountry = dto.ShippingCountry,
            PaymentMethod = dto.PaymentMethod,
            CustomerNote = dto.CustomerNote,
            Status = OrderStatus.Pending,
            PaymentStatus = PaymentStatus.Unpaid,
            OrderItems = dto.Items.Select(i => new OrderItem
            {
                ProductId = i.ProductId,
                VariantId = i.VariantId,
                ProductName = i.ProductName,
                VariantName = i.VariantName,
                SKU = i.SKU,
                ImageUrl = i.ImageUrl,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                Discount = i.Discount,
                Total = (i.UnitPrice - i.Discount) * i.Quantity
            }).ToList()
        };

        order.SubTotal = order.OrderItems.Sum(i => i.Total);
        order.Total = order.SubTotal + order.ShippingFee - order.Discount + order.Tax;

        var created = await _orderRepo.InsertOrderAsync(order);

        // Log initial status
        await _orderRepo.InsertOrderHistoryAsync(new OrderHistory
        {
            OrderId = created.ItemID,
            ToStatus = OrderStatus.Pending,
            Comment = "Order created",
            IsSystemAction = true,
            ChangedByUserId = operatorUserId
        });

        await _eventLog.LogInformation("OrderService", "ORDER_CREATED",
            $"Order {created.OrderCode} created for {created.CustomerEmail}");

        return MapToDTO(created);
    }

    public async Task<OrderGetDTO> UpdateOrderStatusAsync(int orderId, OrderUpdateStatusDTO dto, int? operatorUserId = null)
    {
        var order = await _orderRepo.GetOrderAsync(orderId)
            ?? throw new KeyNotFoundException($"Order {orderId} not found");

        var prevStatus = order.Status;

        // Cancel shipment on ShipRelay before persisting — fire and forget errors (admin can resolve manually)
        if (dto.NewStatus == OrderStatus.Cancelled && !string.IsNullOrEmpty(order.ShiprelayShipmentId))
        {
            var cancelled = await _shiprelayService.CancelShipmentAsync(order.ShiprelayShipmentId);
            if (!cancelled)
                await _eventLog.LogWarning("OrderService", "SHIPRELAY_CANCEL_FAILED",
                    $"Failed to cancel ShipRelay shipment {order.ShiprelayShipmentId} for OrderId={orderId}");
        }

        var updated = await _orderRepo.UpdateOrderAsync(orderId, o =>
        {
            o.Status = dto.NewStatus;
            if (dto.NewStatus == OrderStatus.Shipped) o.ShippedAt = DateTimeOffset.UtcNow;
            if (dto.NewStatus == OrderStatus.Delivered || dto.NewStatus == OrderStatus.Completed)
                o.DeliveredAt = DateTimeOffset.UtcNow;
        });

        await _orderRepo.InsertOrderHistoryAsync(new OrderHistory
        {
            OrderId = orderId,
            FromStatus = prevStatus,
            ToStatus = dto.NewStatus,
            Comment = dto.Comment,
            ChangedByUserId = operatorUserId,
            IsSystemAction = false
        });

        await _eventLog.LogInformation("OrderService", "ORDER_STATUS_UPDATED",
            $"Order {order.OrderCode}: {prevStatus} → {dto.NewStatus}");

        return MapToDTO(updated);
    }

    public async Task<OrderGetDTO> UpdateOrderTrackingAsync(int orderId, OrderUpdateTrackingDTO dto, int? operatorUserId = null)
    {
        var order = await _orderRepo.GetOrderAsync(orderId)
            ?? throw new KeyNotFoundException($"Order {orderId} not found");

        var updated = await _orderRepo.UpdateOrderAsync(orderId, o =>
        {
            if (dto.TrackingNumber is not null) o.TrackingNumber = dto.TrackingNumber;
            if (dto.TrackingUrl is not null) o.TrackingUrl = dto.TrackingUrl;
            if (dto.ShippingCarrier is not null) o.ShippingCarrier = dto.ShippingCarrier;
            if (dto.ShiprelayShipmentId is not null) o.ShiprelayShipmentId = dto.ShiprelayShipmentId;
        });

        await _orderRepo.InsertOrderHistoryAsync(new OrderHistory
        {
            OrderId = orderId,
            ToStatus = order.Status,
            Comment = $"Tracking updated: {dto.TrackingNumber ?? "—"} ({dto.ShippingCarrier ?? "—"})",
            ChangedByUserId = operatorUserId,
            IsSystemAction = false
        });

        await _eventLog.LogInformation("OrderService", "ORDER_TRACKING_UPDATED",
            $"Order {order.OrderCode}: tracking updated by userId={operatorUserId}");

        return MapToDTO(updated);
    }

    public async Task<ShiprelayTrackingResult?> GetOrderTrackingAsync(int orderId)
    {
        var order = await _orderRepo.GetOrderAsync(orderId)
            ?? throw new KeyNotFoundException($"Order {orderId} not found");

        if (string.IsNullOrEmpty(order.ShiprelayShipmentId))
            return null;

        return await _shiprelayService.GetTrackingAsync(order.ShiprelayShipmentId);
    }

    public async Task<bool> DeleteOrderAsync(int orderId)
        => await _orderRepo.DeleteOrderAsync(orderId);

    // ── History ───────────────────────────────────────────────────────────

    public async Task<IEnumerable<OrderHistoryGetDTO>> GetOrderHistoriesAsync(int orderId)
    {
        var histories = await _orderRepo.GetOrderHistoriesAsync(orderId);
        return histories.Select(h => new OrderHistoryGetDTO
        {
            HistoryId = h.ItemID,
            FromStatus = h.FromStatus,
            ToStatus = h.ToStatus,
            Comment = h.Comment,
            ChangedByName = h.ChangedByName,
            IsSystemAction = h.IsSystemAction,
            CreatedAt = h.CreatedAt
        });
    }

    // ── Notes ─────────────────────────────────────────────────────────────

    public async Task<IEnumerable<OrderNoteGetDTO>> GetOrderNotesAsync(int orderId)
    {
        var notes = await _orderRepo.GetOrderNotesAsync(orderId);
        return notes.Select(n => new OrderNoteGetDTO
        {
            NoteId = n.ItemID,
            Note = n.Note,
            AuthorName = n.AuthorName,
            CreatedAt = n.CreatedAt
        });
    }

    public async Task<OrderNoteGetDTO> AddOrderNoteAsync(int orderId, OrderNoteCreateDTO dto, int authorId, string authorName)
    {
        var note = await _orderRepo.InsertOrderNoteAsync(new OrderNote
        {
            OrderId = orderId,
            Note = dto.Note,
            AuthorId = authorId,
            AuthorName = authorName
        });
        return new OrderNoteGetDTO
        {
            NoteId = note.ItemID,
            Note = note.Note,
            AuthorName = note.AuthorName,
            CreatedAt = note.CreatedAt
        };
    }

    public Task<bool> DeleteOrderNoteAsync(int noteId)
        => _orderRepo.DeleteOrderNoteAsync(noteId);

    // ── Returns ───────────────────────────────────────────────────────────

    public async Task<IEnumerable<OrderReturnGetDTO>> GetOrderReturnsAsync(int orderId)
    {
        var returns = await _orderRepo.GetOrderReturnsAsync(orderId);
        return returns.Select(MapReturnToDTO);
    }

    public async Task<OrderReturnGetDTO?> GetOrderReturnAsync(int returnId)
    {
        var ret = await _orderRepo.GetOrderReturnAsync(returnId);
        return ret == null ? null : MapReturnToDTO(ret);
    }

    public async Task<PagedResponse<OrderReturnGetDTO>> GetAllReturnsAsync(int page, int pageSize, ReturnStatus? status, string? search)
    {
        var paged = await _orderRepo.GetAllReturnsAsync(page, pageSize, status, search);
        var dtos = paged.Select(MapReturnToDTO).ToList().AsPagedEnumerable(paged.TotalRecords);
        return PagedResponse<OrderReturnGetDTO>.Success(dtos, page, pageSize);
    }

    public async Task<OrderReturnGetDTO> CreateReturnAsync(int orderId, OrderReturnCreateDTO dto)
    {
        var order = await _orderRepo.GetOrderAsync(orderId)
            ?? throw new KeyNotFoundException($"Order {orderId} not found");

        var ret = new OrderReturn
        {
            OrderId = orderId,
            Reason = dto.Reason,
            Status = ReturnStatus.Pending,
            ReturnItems = dto.Items.Select(i => new OrderReturnItem
            {
                OrderItemId = i.OrderItemId,
                Quantity = i.Quantity,
                Reason = i.Reason
            }).ToList()
        };

        var created = await _orderRepo.InsertOrderReturnAsync(ret);

        // Mark order as return requested
        await _orderRepo.UpdateOrderAsync(orderId, o => o.Status = OrderStatus.ReturnRequested);
        await _orderRepo.InsertOrderHistoryAsync(new OrderHistory
        {
            OrderId = orderId,
            FromStatus = order.Status,
            ToStatus = OrderStatus.ReturnRequested,
            Comment = $"Return requested: {dto.Reason}",
            IsSystemAction = false
        });

        return MapReturnToDTO(created);
    }

    public async Task<OrderReturnGetDTO> ReviewReturnAsync(int returnId, OrderReturnReviewDTO dto, int reviewerId, string reviewerName)
    {
        var ret = await _orderRepo.GetOrderReturnAsync(returnId)
            ?? throw new KeyNotFoundException($"OrderReturn {returnId} not found");

        var updated = await _orderRepo.UpdateOrderReturnAsync(returnId, r =>
        {
            r.Status = dto.Decision;
            r.AdminNote = dto.AdminNote;
            r.RefundAmount = dto.RefundAmount;
            r.ReviewedByUserId = reviewerId;
            r.ReviewedAt = DateTimeOffset.UtcNow;
        });

        // Update order status
        var newOrderStatus = dto.Decision == ReturnStatus.Approved || dto.Decision == ReturnStatus.Refunded
            ? OrderStatus.Returned
            : OrderStatus.Completed;

        await _orderRepo.UpdateOrderAsync(ret.OrderId, o => o.Status = newOrderStatus);
        await _orderRepo.InsertOrderHistoryAsync(new OrderHistory
        {
            OrderId = ret.OrderId,
            ToStatus = newOrderStatus,
            Comment = $"Return {dto.Decision} by {reviewerName}. {dto.AdminNote}",
            ChangedByUserId = reviewerId,
            ChangedByName = reviewerName,
            IsSystemAction = false
        });

        await _eventLog.LogInformation("OrderService", "RETURN_REVIEWED",
            $"Return #{returnId} reviewed: {dto.Decision} by {reviewerName}");

        return MapReturnToDTO(updated);
    }

    // ── Mappers ───────────────────────────────────────────────────────────

    private static OrderGetDTO MapToDTO(Order o) => new()
    {
        OrderId = o.ItemID,
        OrderCode = o.OrderCode,
        CustomerName = o.CustomerName,
        CustomerEmail = o.CustomerEmail,
        CustomerPhone = o.CustomerPhone,
        ShippingAddress = o.ShippingAddress,
        ShippingCity = o.ShippingCity,
        ShippingState = o.ShippingState,
        ShippingZip = o.ShippingZip,
        ShippingCountry = o.ShippingCountry,
        SubTotal = o.SubTotal,
        ShippingFee = o.ShippingFee,
        Discount = o.Discount,
        Tax = o.Tax,
        Total = o.Total,
        Status = o.Status,
        PaymentStatus = o.PaymentStatus,
        PaymentMethod = o.PaymentMethod,
        TrackingNumber = o.TrackingNumber,
        TrackingUrl = o.TrackingUrl,
        ShippingCarrier = o.ShippingCarrier,
        ShiprelayShipmentId = o.ShiprelayShipmentId,
        CustomerNote = o.CustomerNote,
        ShippedAt = o.ShippedAt,
        DeliveredAt = o.DeliveredAt,
        CreatedAt = o.CreatedAt,
        UpdatedAt = o.UpdatedAt,
        Items = o.OrderItems.Select(i => new OrderItemGetDTO
        {
            ItemId = i.ItemID,
            ProductId = i.ProductId,
            VariantId = i.VariantId,
            ProductName = i.ProductName,
            VariantName = i.VariantName,
            SKU = i.SKU,
            ImageUrl = i.ImageUrl,
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice,
            Discount = i.Discount,
            Total = i.Total
        }).ToList()
    };

    private static OrderReturnGetDTO MapReturnToDTO(OrderReturn r) => new()
    {
        ReturnId = r.ItemID,
        OrderId = r.OrderId,
        OrderCode = r.Order?.OrderCode ?? "",
        Reason = r.Reason,
        Status = r.Status,
        RefundAmount = r.RefundAmount,
        AdminNote = r.AdminNote,
        ReviewedByName = r.ReviewedBy != null
            ? $"{r.ReviewedBy.FirstName} {r.ReviewedBy.LastName}".Trim()
            : null,
        ReviewedAt = r.ReviewedAt,
        CreatedAt = r.CreatedAt,
        Items = r.ReturnItems.Select(i => new ReturnItemGetDTO
        {
            OrderItemId = i.OrderItemId,
            ProductName = i.OrderItem?.ProductName ?? "",
            SKU = i.OrderItem?.SKU,
            Quantity = i.Quantity,
            Reason = i.Reason
        }).ToList()
    };

    private static string GenerateOrderCode()
        => $"ORD-{DateTimeOffset.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}";
}
