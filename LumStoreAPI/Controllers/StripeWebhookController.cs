using LumStoreAPI.Application.Interfaces;
using LumStoreAPI.Core.Interfaces.Sytems;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;

namespace LumStoreAPI.Controllers
{
    [Route("api/webhooks/stripe")]
    [ApiExplorerSettings(GroupName = "Webhook")]
    [ApiController]
    public class StripeWebhookController : ControllerBase
    {
        private readonly IEventLogService _eventLogService;
        private readonly IStripeClient _stripeClient;
        private readonly IOrderService _orderService;
        public StripeWebhookController(IEventLogService eventLogService, IStripeClient stripeClient, IOrderService orderService)
        {
            _eventLogService = eventLogService;
            _stripeClient = stripeClient;
            _orderService = orderService;
        }
        [HttpPost]
        public async Task<IActionResult> Index()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            try
            {
                var stripeEvent = EventUtility.ParseEvent(json);
                if (stripeEvent.Type == EventTypes.CheckoutSessionAsyncPaymentSucceeded)
                {
                    var sessionData = stripeEvent.Data.Object as Session;
                    if (sessionData is null || !int.TryParse(sessionData.ClientReferenceId, out int orderId))
                    {
                        await _eventLogService.LogEvent(Core.Models.Enums.EventLogType.ERROR, "WEBHOOK", "Stripe", "Data null");
                        return BadRequest("Data null");
                    }

                    long amountRaw = sessionData.AmountTotal ?? 0;
                    string currency = sessionData.Currency.ToUpper();
                    var service = new PaymentIntentService(_stripeClient);
                    var paymentIntent = await service.GetAsync(sessionData.PaymentIntentId);
                    string actualMethod = paymentIntent.PaymentMethod.Type;

                    string note = $"Paid via Stripe with {actualMethod} method: {amountRaw.ToString("C")} {currency}";

                    await _orderService.UpdateOrderStatusAsync(orderId, new() { Comment = note, NewStatus = Core.Models.Enums.OrderStatus.Confirmed });
                }

                return Ok();
            }
            catch (StripeException ex)
            {
                await _eventLogService.LogException("WEBHOOK", "Stripe", "Failed to get webhook", ex);
                return BadRequest("Failed to get webhook");
            }
        }
    }
}
