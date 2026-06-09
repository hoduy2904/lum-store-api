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
                    UnitAmount = (long)Math.Round(item.UnitPrice * 100),
                    Currency = "usd",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = item.ProductName,
                        Description = item.VariantName,
                    },
                },
                Quantity = item.Quantity,
            }).ToList();

            if (request.ShippingFee > 0)
                lineItems.Add(new SessionLineItemOptions
                {
                    Quantity = 1,
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)Math.Round(request.ShippingFee * 100),
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = "Shipping fee"
                        }
                    }
                });

            if (request.Order.Tax > 0)
                lineItems.Add(new SessionLineItemOptions
                {
                    Quantity = 1,
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)Math.Round(request.Order.Tax * 100),
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = "Tax fee"
                        }
                    }
                });

            var options = new SessionCreateOptions
            {
                LineItems = lineItems,
                Mode = "payment",
                SuccessUrl = request.SuccessUrl,
                CancelUrl = request.CancelUrl,
                ClientReferenceId = request.Order.ItemID + "",
                Metadata = new Dictionary<string, string>
                {
                    ["order_code"] = request.Order.OrderCode
                },
                CustomerCreation = request.Order.CustomerName,
                CustomerEmail = request.Order.CustomerEmail
            };

            var service = new SessionService(_stripeClient);
            var session = await service.CreateAsync(options);
            return session.Url;
        }
    }
}
