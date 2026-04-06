using System.ComponentModel.DataAnnotations;

namespace LumStoreAPI.Application.DTOs.OrderDTO;

public class OrderCreateDTO
{
    public int? CustomerId { get; set; }

    [Required, MaxLength(150)]
    public string CustomerName { get; set; } = default!;

    [Required, EmailAddress, MaxLength(200)]
    public string CustomerEmail { get; set; } = default!;

    [MaxLength(30)]
    public string CustomerPhone { get; set; } = default!;

    [Required, MaxLength(500)]
    public string ShippingAddress { get; set; } = default!;

    [Required, MaxLength(100)]
    public string ShippingCity { get; set; } = default!;

    [MaxLength(100)]
    public string ShippingState { get; set; } = default!;

    [MaxLength(20)]
    public string ShippingZip { get; set; } = default!;

    [MaxLength(10)]
    public string ShippingCountry { get; set; } = "US";

    [MaxLength(50)]
    public string? PaymentMethod { get; set; }

    [MaxLength(1000)]
    public string? CustomerNote { get; set; }

    [Required, MinLength(1)]
    public List<OrderItemCreateDTO> Items { get; set; } = [];
}

public class OrderItemCreateDTO
{
    [Required]
    public int ProductId { get; set; }
    public int? VariantId { get; set; }

    [Required, MaxLength(200)]
    public string ProductName { get; set; } = default!;
    public string? VariantName { get; set; }
    public string? SKU { get; set; }
    public string? ImageUrl { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    [Range(0, double.MaxValue)]
    public decimal UnitPrice { get; set; }
    public decimal Discount { get; set; }
}
