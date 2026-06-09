using LumStoreAPI.Application.Interfaces;
using LumStoreAPI.Core.Interfaces.Sytems;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace LumStoreAPI.Controllers
{
    [Route("api/webhooks/stripe")]
    [ApiExplorerSettings(GroupName = "Webhook")]
    [ApiController]
    public class StripeWebhookController : ControllerBase
    {
        private readonly IEventLogService _eventLogService;
        private readonly IStripeWebhookService _webhookService;

        public StripeWebhookController(IEventLogService eventLogService, IStripeWebhookService webhookService)
        {
            _eventLogService = eventLogService;
            _webhookService = webhookService;
        }

        [HttpPost]
        public async Task<IActionResult> Index()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var signature = Request.Headers["Stripe-Signature"].FirstOrDefault() ?? string.Empty;

            if (!await _webhookService.ValidateSignatureAsync(json, signature))
            {
                await _eventLogService.LogWarning("STRIPE_WEBHOOK", "INVALID_SIGNATURE",
                    "Stripe webhook signature validation failed");
                return Unauthorized("Invalid signature");
            }

            try
            {
                await _webhookService.ProcessWebhookAsync(json);
                return Ok();
            }
            catch (StripeException ex)
            {
                await _eventLogService.LogException("STRIPE_WEBHOOK", "PROCESSING_FAILED",
                    "Failed to process Stripe webhook", ex);
                return BadRequest("Failed to process webhook");
            }
        }
    }
}
