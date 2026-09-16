using LumStoreAPI.Application.DTOs.PaymentDTO;
using LumStoreAPI.Application.Interfaces;
using LumStoreAPI.Core.Entities.Orders;
using LumStoreAPI.Core.Interfaces.Sytems;
using LumStoreAPI.Core.Models.Constants;
using LumStoreAPI.Core.Models.Enums;
using LumStoreAPI.Libraries.Helpers;
using Stripe;
using Stripe.Checkout;

namespace LumStoreAPI.Application.Services
{
    internal class PaymentService : IPaymentService
    {
        private readonly IStripeClient _stripeClient;
        private readonly IEventLogService _eventLogService;
        private readonly IEmailService _emailService;
        public PaymentService(IStripeClient stripeClient, IEventLogService eventLogService, IEmailService emailService)
        {
            _stripeClient = stripeClient;
            _eventLogService = eventLogService;
            _emailService = emailService;
        }

        public async Task<Refund> CreateRefundAsync(ReturnRequestDTO request, Order? order = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.PaymentIntentId)) throw new ArgumentNullException(nameof(request.PaymentIntentId), "payment intent cannot null");
                var service = new RefundService(_stripeClient);
                // Amount is part of the key: Stripe rejects a reused key with different parameters,
                // so without it a failed refund could not be retried with a corrected amount
                var idempotencyKey = "refund_" + request.OrderCode + "_" + request.RefundId + "_" + (request.Amount?.ToString() ?? "full");
                var refund = await service.CreateAsync(new()
                {
                    PaymentIntent = request.PaymentIntentId,
                    Reason = "requested_by_customer",
                    Metadata = new Dictionary<string, string>()
                    {
                        ["order_code"] = request.OrderCode,
                        ["refund_id"] = request.RefundId.ToString()
                    },
                    Amount = request.Amount,
                }, new() { IdempotencyKey = idempotencyKey });
                if (order is not null && refund is not null)
                {
                    var cancelEmailUser = await EmailHelper.MacroEmailTemplate(EmailTemplateConstant.USER_CANCELLED_ORDER, order);
                    var cancelEmailAdmin = await EmailHelper.MacroEmailTemplate(EmailTemplateConstant.ADMIN_CANCELLED_ORDER, order);
                    await _emailService.SendEmailAsync(new Core.Models.Systems.EmailMessage
                    {
                        EmailSubject = cancelEmailUser.EmailHeader,
                        EmailBody = cancelEmailUser.EmailBody,
                        EmailTo = [order.CustomerEmail]
                    });
                    await _emailService.SendEmailAsync(new Core.Models.Systems.EmailMessage
                    {
                        EmailSubject = cancelEmailAdmin.EmailHeader,
                        EmailBody = cancelEmailAdmin.EmailBody,
                    }, true);
                }
                return refund;
            }
            catch (Exception ex)
            {
                await _eventLogService.LogException("Payment", "REFUND", "Refund Failed", ex);
                throw;
            }
        }

        public async Task<string> PaymentCheckoutAsync(PaymentRequestDTO request)
        {

            var lineItems = request.Order.OrderItems.Select(item => new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    // OrderItem.UnitPrice is the price BEFORE discount — charge the discounted unit price
                    UnitAmount = (long)Math.Round((item.UnitPrice - item.Discount) * 100, MidpointRounding.AwayFromZero),
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

            // Stripe charges the sum of line items — it must equal Orders.Total
            var expectedAmount = (long)Math.Round(request.Order.Total * 100, MidpointRounding.AwayFromZero);
            var chargedAmount = lineItems.Sum(x => (x.PriceData.UnitAmount ?? 0) * (x.Quantity ?? 0));
            if (chargedAmount != expectedAmount)
                await _eventLogService.LogEvent(EventLogType.ERROR, "PAYMENT", "AMOUNT_MISMATCH",
                    $"Order {request.Order.OrderCode}: Stripe amount does not match order total",
                    $"Expected: {expectedAmount / 100m:F2} USD, actual: {chargedAmount / 100m:F2} USD");

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
                CustomerCreation = "if_required",
                CustomerEmail = request.Order.CustomerEmail
            };

            var service = new SessionService(_stripeClient);
            var session = await service.CreateAsync(options);
            return session.Url;
        }
    }
}
