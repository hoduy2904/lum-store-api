using LumStoreAPI.Core.Models.Enums;

namespace LumStoreAPI.Application.DTOs.OrderDTO;

public class OrderListRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public OrderStatus? Status { get; set; }
    public PaymentStatus? PaymentStatus { get; set; }
    public string? Search { get; set; }         // Search by OrderCode, customer name, email
    public int? CustomerId { get; set; }
    public DateTimeOffset? FromDate { get; set; }
    public DateTimeOffset? ToDate { get; set; }
    public string SortBy { get; set; } = "CreatedAt";
    public bool Descending { get; set; } = true;
}
