using LumStoreAPI.Application.DTOs.OrderDTO;
using LumStoreAPI.Application.DTOs.Responses;
using LumStoreAPI.Application.DTOs.ShiprelayDTO;
using LumStoreAPI.Application.Interfaces;
using LumStoreAPI.Core.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LumStoreAPI.Controllers;

/// <summary>Order management — admin CRUD, status updates, notes, returns.</summary>
[Route("api/orders")]
[ApiController]
[Authorize(Roles = "ADMIN")]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    // ── CRUD ──────────────────────────────────────────────────────────────

    /// <summary>GET /api/orders — Paginated list with filters.</summary>
    [HttpGet]
    public async Task<IActionResult> GetOrders([FromQuery] OrderListRequest request)
    {
        var result = await _orderService.GetOrdersAsync(request);
        return Ok(result);
    }

    /// <summary>GET /api/orders/{orderId}</summary>
    [HttpGet("{orderId:int}")]
    public async Task<IActionResult> GetOrder(int orderId)
    {
        var order = await _orderService.GetOrderAsync(orderId);
        if (order == null) return NotFound(APIResponseBase.Failure("NOT_FOUND", [$"Order {orderId} not found"]));
        return Ok(APIResponse<OrderGetDTO>.Success(order));
    }

    /// <summary>GET /api/orders/code/{orderCode}</summary>
    [HttpGet("code/{orderCode}")]
    public async Task<IActionResult> GetOrderByCode(string orderCode)
    {
        var order = await _orderService.GetOrderByCodeAsync(orderCode);
        if (order == null) return NotFound(APIResponseBase.Failure("NOT_FOUND", [$"Order {orderCode} not found"]));
        return Ok(APIResponse<OrderGetDTO>.Success(order));
    }

    /// <summary>POST /api/orders — Create a new order.</summary>
    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] OrderCreateDTO dto)
    {
        var operatorId = GetCurrentUserId();
        var created = await _orderService.CreateOrderAsync(dto, operatorId);
        return CreatedAtAction(nameof(GetOrder), new { orderId = created.OrderId },
            APIResponse<OrderGetDTO>.Success(created, ["Order created successfully"]));
    }

    /// <summary>PATCH /api/orders/{orderId}/status — Update order status.</summary>
    [HttpPatch("{orderId:int}/status")]
    public async Task<IActionResult> UpdateStatus(int orderId, [FromBody] OrderUpdateStatusDTO dto)
    {
        var operatorId = GetCurrentUserId();
        var updated = await _orderService.UpdateOrderStatusAsync(orderId, dto, operatorId);
        return Ok(APIResponse<OrderGetDTO>.Success(updated, ["Status updated"]));
    }

    /// <summary>GET /api/orders/{orderId}/tracking — Pull live tracking from ShipRelay.</summary>
    [HttpGet("{orderId:int}/tracking")]
    public async Task<IActionResult> GetTracking(int orderId)
    {
        var tracking = await _orderService.GetOrderTrackingAsync(orderId);
        if (tracking == null)
            return NotFound(APIResponseBase.Failure("NO_SHIPMENT", [$"Order {orderId} has no ShipRelay shipment linked"]));
        return Ok(APIResponse<ShiprelayTrackingResult>.Success(tracking));
    }

    /// <summary>PATCH /api/orders/{orderId}/tracking — Manually update tracking fields.</summary>
    [HttpPatch("{orderId:int}/tracking")]
    public async Task<IActionResult> UpdateTracking(int orderId, [FromBody] OrderUpdateTrackingDTO dto)
    {
        var operatorId = GetCurrentUserId();
        var updated = await _orderService.UpdateOrderTrackingAsync(orderId, dto, operatorId);
        return Ok(APIResponse<OrderGetDTO>.Success(updated, ["Tracking updated"]));
    }

    /// <summary>DELETE /api/orders/{orderId}</summary>
    [HttpDelete("{orderId:int}")]
    public async Task<IActionResult> DeleteOrder(int orderId)
    {
        var deleted = await _orderService.DeleteOrderAsync(orderId);
        if (!deleted) return NotFound(APIResponseBase.Failure("NOT_FOUND", [$"Order {orderId} not found"]));
        return Ok(APIResponseBase.Success(["Order deleted"]));
    }

    // ── History ───────────────────────────────────────────────────────────

    /// <summary>GET /api/orders/{orderId}/history</summary>
    [HttpGet("{orderId:int}/history")]
    public async Task<IActionResult> GetHistory(int orderId)
    {
        var histories = await _orderService.GetOrderHistoriesAsync(orderId);
        return Ok(APIResponse<IEnumerable<OrderHistoryGetDTO>>.Success(histories));
    }

    // ── Notes ─────────────────────────────────────────────────────────────

    /// <summary>GET /api/orders/{orderId}/notes</summary>
    [HttpGet("{orderId:int}/notes")]
    public async Task<IActionResult> GetNotes(int orderId)
    {
        var notes = await _orderService.GetOrderNotesAsync(orderId);
        return Ok(APIResponse<IEnumerable<OrderNoteGetDTO>>.Success(notes));
    }

    /// <summary>POST /api/orders/{orderId}/notes</summary>
    [HttpPost("{orderId:int}/notes")]
    public async Task<IActionResult> AddNote(int orderId, [FromBody] OrderNoteCreateDTO dto)
    {
        var (userId, userName) = GetCurrentUserInfo();
        var note = await _orderService.AddOrderNoteAsync(orderId, dto, userId ?? 0, userName);
        return Ok(APIResponse<OrderNoteGetDTO>.Success(note, ["Note added"]));
    }

    /// <summary>DELETE /api/orders/notes/{noteId}</summary>
    [HttpDelete("notes/{noteId:int}")]
    public async Task<IActionResult> DeleteNote(int noteId)
    {
        var deleted = await _orderService.DeleteOrderNoteAsync(noteId);
        if (!deleted) return NotFound(APIResponseBase.Failure("NOT_FOUND", ["Note not found"]));
        return Ok(APIResponseBase.Success(["Note deleted"]));
    }

    // ── Returns ───────────────────────────────────────────────────────────

    /// <summary>GET /api/orders/{orderId}/returns</summary>
    [HttpGet("{orderId:int}/returns")]
    public async Task<IActionResult> GetReturns(int orderId)
    {
        var returns = await _orderService.GetOrderReturnsAsync(orderId);
        return Ok(APIResponse<IEnumerable<OrderReturnGetDTO>>.Success(returns));
    }

    /// <summary>GET /api/orders/returns — Paginated list of all returns (admin).</summary>
    [HttpGet("returns")]
    public async Task<IActionResult> GetAllReturns(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] ReturnStatus? status = null,
        [FromQuery] string? search = null)
    {
        var result = await _orderService.GetAllReturnsAsync(page, pageSize, status, search);
        return Ok(result);
    }

    /// <summary>GET /api/orders/returns/{returnId}</summary>
    [HttpGet("returns/{returnId:int}")]
    public async Task<IActionResult> GetReturn(int returnId)
    {
        var ret = await _orderService.GetOrderReturnAsync(returnId);
        if (ret == null) return NotFound(APIResponseBase.Failure("NOT_FOUND", ["Return not found"]));
        return Ok(APIResponse<OrderReturnGetDTO>.Success(ret));
    }

    /// <summary>POST /api/orders/{orderId}/returns — Create return request.</summary>
    [HttpPost("{orderId:int}/returns")]
    public async Task<IActionResult> CreateReturn(int orderId, [FromBody] OrderReturnCreateDTO dto)
    {
        var ret = await _orderService.CreateReturnAsync(orderId, dto);
        return Ok(APIResponse<OrderReturnGetDTO>.Success(ret, ["Return request created"]));
    }

    /// <summary>PATCH /api/orders/returns/{returnId}/review — Approve or reject return.</summary>
    [HttpPatch("returns/{returnId:int}/review")]
    public async Task<IActionResult> ReviewReturn(int returnId, [FromBody] OrderReturnReviewDTO dto)
    {
        var (userId, userName) = GetCurrentUserInfo();
        var ret = await _orderService.ReviewReturnAsync(returnId, dto, userId ?? 0, userName);
        return Ok(APIResponse<OrderReturnGetDTO>.Success(ret, ["Return reviewed"]));
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private int? GetCurrentUserId()
    {
        if (int.TryParse(User.Claims.FirstOrDefault(c => c.Type == "id")?.Value, out int id))
            return id;
        return null;
    }

    private (int? id, string name) GetCurrentUserInfo()
    {
        int.TryParse(User.Claims.FirstOrDefault(c => c.Type == "id")?.Value, out int id);
        var name = User.Claims.FirstOrDefault(c => c.Type == "name")?.Value ?? "Unknown";
        return (id > 0 ? id : null, name);
    }
}
