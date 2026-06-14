using LumStoreAPI.Application.DTOs.PaymentDTO;
using LumStoreAPI.Core.Entities.Orders;
using Stripe;

namespace LumStoreAPI.Application.Interfaces
{
    public interface IPaymentService
    {
        Task<string> PaymentCheckoutAsync(PaymentRequestDTO request);
        /// <summary>
        /// If order not full then email will send
        /// </summary>
        /// <param name="request"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        Task<Refund?> CreateRefundAsync(ReturnRequestDTO request, Order? order = null);
    }
}
