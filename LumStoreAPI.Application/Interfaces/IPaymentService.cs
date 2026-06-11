using LumStoreAPI.Application.DTOs.PaymentDTO;
using Stripe;

namespace LumStoreAPI.Application.Interfaces
{
    public interface IPaymentService
    {
        Task<string> PaymentCheckoutAsync(PaymentRequestDTO request);
        Task<Refund?> CreateRefundAsync(ReturnRequestDTO request);
    }
}
