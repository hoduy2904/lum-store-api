using LumStoreAPI.Core.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace LumStoreAPI.Application.DTOs.OrderDTO;

public class OrderReturnCreateDTO
{
    [Required, MaxLength(1000)]
    public string Reason { get; set; } = default!;
    public List<ReturnItemDTO> Items { get; set; } = [];
    public string? StripeRefundId { get; set; }
}

public class ReturnItemDTO
{
    public int OrderItemId { get; set; }
    public int Quantity { get; set; }
    public string? Reason { get; set; }
}

public class OrderReturnReviewDTO
{
    [Required]
    public ReturnStatus Decision { get; set; }

    [MaxLength(1000)]
    public string? AdminNote { get; set; }

    public decimal RefundAmount { get; set; }
}

public class OrderReturnGetDTO
{
    public int ReturnId { get; set; }
    public int OrderId { get; set; }
    public string OrderCode { get; set; } = default!;
    public string? CustomerName { get; set; }
    public string? CustomerEmail { get; set; }
    public string Reason { get; set; } = default!;
    public ReturnStatus Status { get; set; }
    public string StatusName => Status.ToString();
    public decimal RefundAmount { get; set; }
    public string? AdminNote { get; set; }
    public string? ReviewedByName { get; set; }
    public DateTimeOffset? ReviewedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public List<ReturnItemGetDTO> Items { get; set; } = [];
}

public class ReturnItemGetDTO
{
    public int OrderItemId { get; set; }
    public string ProductName { get; set; } = default!;
    public string? SKU { get; set; }
    public int Quantity { get; set; }
    public string? Reason { get; set; }
}
