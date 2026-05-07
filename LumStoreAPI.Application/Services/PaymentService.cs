using LumStoreAPI.Application.DTOs.PaymentDTO;
using LumStoreAPI.Application.Interfaces;
using LumStoreAPI.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Stripe.Checkout;

namespace LumStoreAPI.Application.Services
{
    internal class PaymentService : IPaymentService
    {
        private readonly IStripeClient _stripeClient;
        public PaymentService(IStripeClient stripeClient)
        {
            _stripeClient = stripeClient;
        }
        public async Task<string> PaymentCheckoutAsync(PaymentRequestDTO request)
        {
            var lineItems = request.Order.OrderItems.Select(item => new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    UnitAmountDecimal = item.Total,
                    Currency = "usd",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = item.ProductName,
                        Description = item.VariantName,
                    }
                },
                Quantity = item.Quantity,
            });

            lineItems.Append(new SessionLineItemOptions
            {
                Quantity = 1,
                PriceData = new SessionLineItemPriceDataOptions
                {
                    UnitAmountDecimal = request.ShippingFee,
                    Currency = "usd",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = "Shipping fee"
                    }
                }
            });

            lineItems.Append(new SessionLineItemOptions
            {
                Quantity = 1,
                PriceData = new SessionLineItemPriceDataOptions
                {
                    UnitAmountDecimal = request.Order.Tax,
                    Currency = "usd",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = "Tax fee"
                    }
                }
            });

            var options = new SessionCreateOptions
            {
                LineItems = lineItems.ToList(),
                Mode = "payment",
                SuccessUrl = request.SuccessUrl,
                CancelUrl = request.CancelUrl,
                ClientReferenceId = request.Order.ItemID + "",
            };

            var service = new SessionService(_stripeClient);
            var session = await service.CreateAsync(options);
            return session.Url;
        }
    }
}
