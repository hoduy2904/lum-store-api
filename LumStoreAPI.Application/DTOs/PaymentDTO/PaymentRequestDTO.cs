using LumStoreAPI.Core.Entities.Orders;

namespace LumStoreAPI.Application.DTOs.PaymentDTO
{
    public class PaymentRequestDTO
    {
        public required Order Order { get; set; }
        public string SuccessUrl { get; set; } = default!;
        public string? CancelUrl { get; set; }
    }
}
