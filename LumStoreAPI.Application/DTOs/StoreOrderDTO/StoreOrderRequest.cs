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

    /// <summary>Mã dịch vụ vận chuyển từ ShipRelay (e.g. "SR_STANDARD"). Nếu null dùng option đầu tiên.</summary>
    public string? ShippingServiceCode { get; set; }
}

public class StoreCheckoutPreviewRequest
{
    [Required]
    public int AddressId { get; set; }

    /// <summary>Mã dịch vụ vận chuyển muốn xem giá (e.g. "SR_STANDARD"). Nếu null trả về option đầu tiên làm mặc định.</summary>
    public string? ShippingServiceCode { get; set; }
}
