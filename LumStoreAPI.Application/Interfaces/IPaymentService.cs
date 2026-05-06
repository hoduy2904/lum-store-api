using LumStoreAPI.Application.DTOs.CartDTO;
using LumStoreAPI.Application.DTOs.PaymentDTO;

namespace LumStoreAPI.Application.Interfaces
{
    public interface IPaymentService
    {
        Task<string> PaymentCheckoutAsync(PaymentRequestDTO request);
    }
}
