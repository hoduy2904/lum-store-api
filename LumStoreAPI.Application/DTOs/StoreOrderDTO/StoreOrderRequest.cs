using System.ComponentModel.DataAnnotations;

namespace LumStoreAPI.Application.DTOs.StoreOrderDTO;

public class StorePlaceOrderRequest
{
    [Required]
    public int AddressId { get; set; }

    [MaxLength(1000)]
    public string? Note { get; set; }
    public string SuccessUrl { get; set; } = default!;
    public string? CancelUrl { get; set; }
    public string ShippingServiceCode { get; set; } = default!;
}

public class StoreCheckoutPreviewRequest
{
    [Required]
    public int AddressId { get; set; }
}

public class StoreCancelOrderRequest
{
    [MaxLength(500)]
    public string? Reason { get; set; }
}

public class StoreRequestReturnRequest
{
    [MaxLength(1000)]
    public string? Reason { get; set; }
}
