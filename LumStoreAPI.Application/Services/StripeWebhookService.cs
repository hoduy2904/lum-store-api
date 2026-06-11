using LumStoreAPI.Application.DTOs.OrderDTO;
using LumStoreAPI.Application.Interfaces;
using LumStoreAPI.Core.Interfaces.Repositories;
using LumStoreAPI.Core.Interfaces.Sytems;
using LumStoreAPI.Core.Models.Enums;
using LumStoreAPI.Core.Models.Systems;
using LumStoreAPI.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Stripe.Checkout;

namespace LumStoreAPI.Application.Services;

internal class StripeWebhookService : IStripeWebhookService
{
    private readonly LumStoreContext _ctx;
    private readonly IOrderService _orderService;
    private readonly IEmailService _emailService;
    private readonly IEventLogService _eventLogService;
    private readonly IIntegrationConfigRepository _configRepo;
    private readonly IStripeClient _stripeClient;

    public StripeWebhookService(
        LumStoreContext ctx,
        IOrderService orderService,
        IEmailService emailService,
        IEventLogService eventLogService,
        IIntegrationConfigRepository configRepo,
        IStripeClient stripeClient)
    {
        _ctx = ctx;
        _orderService = orderService;
        _emailService = emailService;
        _eventLogService = eventLogService;
        _configRepo = configRepo;
        _stripeClient = stripeClient;
    }

    // ── Signature ─────────────────────────────────────────────────────────────

    public async Task<bool> ValidateSignatureAsync(string json, string signature)
    {
        var config = await _configRepo.GetConfigByTypeAsync(IntegrationType.Payment);
        var secret = config?.WebhookSecret;

        // If no secret is configured, reject all incoming webhooks
        if (string.IsNullOrEmpty(secret)) return false;

        try
        {
            EventUtility.ConstructEvent(json, signature, secret);
            return true;
        }
        catch (StripeException)
        {
            return false;
        }
    }

    // ── Dispatch ──────────────────────────────────────────────────────────────

    public async Task ProcessWebhookAsync(string json)
    {
        var stripeEvent = EventUtility.ParseEvent(json);

        switch (stripeEvent.Type)
        {
            case EventTypes.CheckoutSessionCompleted:
                if (stripeEvent.Data.Object is Session completedSession)
                    await HandlePaymentSuccessAsync(completedSession);
                break;

            case EventTypes.CheckoutSessionExpired:
                if (stripeEvent.Data.Object is Session expiredSession)
                    await HandlePaymentExpiredAsync(expiredSession, "Payment session expired or was cancelled by the customer");
                break;

            case EventTypes.CheckoutSessionAsyncPaymentFailed:
                if (stripeEvent.Data.Object is Session failedSession)
                    await HandlePaymentExpiredAsync(failedSession, "Async payment failed");
                break;
            case EventTypes.RefundUpdated:
                if (stripeEvent.Data.Object is Refund refund)
                    await HandleRefundStatus(refund);
                break;

        }
    }

    // ── Handlers ──────────────────────────────────────────────────────────────

    private async Task HandleRefundStatus(Refund refund)
    {
        if (!refund.Metadata.TryGetValue("refund_id", out string? refundIdString) || !int.TryParse(refundIdString, out int refundId))
        {
            await _eventLogService.LogWarning("WEBHOOK", "STRIPE_REFUND", $"Not found refund ID from {refund.Id}");
            return;
        }

        var refundStatus = GetReturnStatus(refund.Status);
        if (refundStatus == null)
        {
            await _eventLogService.LogWarning("WEBHOOK", "STRIPE_REFUND", "Handle refund status", $"Cannot update refund status with {refund.Status}");
            return;
        }
        await _orderService.UpdateReturnOrderAsync(refundId, new()
        {
            AdminNote = "Refund automatic by Stripe",
            Decision = refundStatus.Value,
            RefundAmount = refund.Amount,
            StripeRefundId = refund.Id,
        });
    }

    private ReturnStatus? GetReturnStatus(string stripeRefund)
    {
        return stripeRefund switch
        {
            "pending" => ReturnStatus.Pending,
            "succeeded" => ReturnStatus.Refunded,
            "canceled" => ReturnStatus.Rejected,
            _ => null
        };
    }
    private async Task HandlePaymentSuccessAsync(Session session)
    {
        if (!int.TryParse(session.ClientReferenceId, out int orderId))
        {
            await _eventLogService.LogEvent(EventLogType.ERROR, "STRIPE_WEBHOOK", "INVALID_ORDER_ID",
                $"checkout.session.completed: ClientReferenceId '{session.ClientReferenceId}' is not a valid order ID");
            return;
        }

        var order = await _ctx.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.ItemID == orderId);

        if (order is null)
        {
            await _eventLogService.LogEvent(EventLogType.ERROR, "STRIPE_WEBHOOK", "ORDER_NOT_FOUND",
                $"checkout.session.completed: Order {orderId} not found");
            return;
        }

        // Idempotency guard — skip if already processed
        if (order.PaymentStatus == PaymentStatus.Paid) return;

        // Resolve payment method label from PaymentIntent (best-effort)
        string paymentMethod = "stripe";
        if (!string.IsNullOrEmpty(session.PaymentIntentId))
        {
            try
            {
                var piService = new PaymentIntentService(_stripeClient);
                var intent = await piService.GetAsync(session.PaymentIntentId);
                paymentMethod = string.Join(',', intent.PaymentMethodTypes ?? ["stripe"]);
            }
            catch (StripeException ex)
            {
                await _eventLogService.LogWarning("STRIPE_WEBHOOK", "PAYMENT_INTENT_FETCH_FAILED",
                    $"Could not fetch PaymentIntent {session.PaymentIntentId}: {ex.Message}");
            }
        }

        long amountRaw = session.AmountTotal ?? 0;
        string currency = session.Currency?.ToUpper() ?? "USD";
        string note = $"Paid via Stripe (payment: {session.PaymentIntentId ?? "N/A"}) ({paymentMethod}): {amountRaw / 100m:F2} {currency}";

        await _orderService.UpdateOrderStatusAsync(orderId, new OrderUpdateStatusDTO
        {
            NewStatus = OrderStatus.Confirmed,
            NewPaymentStatus = PaymentStatus.Paid,
            Comment = note,
            StripePaymentIntentId = session.PaymentIntentId
        });

        // Decrement stock for each variant — clamp at 0 to avoid negative stock
        foreach (var item in order.OrderItems.Where(i => i.VariantId.HasValue))
        {
            await _ctx.ProductVariants
                .Where(v => v.ItemID == item.VariantId!.Value)
                .ExecuteUpdateAsync(s => s.SetProperty(
                    v => v.Stock,
                    v => v.Stock > item.Quantity ? v.Stock - item.Quantity : 0));
        }

        // Enqueue order confirmation email
        await EnqueueOrderConfirmationAsync(order.CustomerEmail, order.CustomerName, order.OrderCode, order.Total);

        await _eventLogService.LogInformation("STRIPE_WEBHOOK", "PAYMENT_SUCCESS",
            $"Order {order.OrderCode} payment confirmed",
            $"Amount: {amountRaw / 100m:F2} {currency}, method: {paymentMethod}");
    }

    private async Task HandlePaymentExpiredAsync(Session session, string reason)
    {
        if (!int.TryParse(session.ClientReferenceId, out int orderId)) return;

        var order = await _ctx.Orders.FirstOrDefaultAsync(o => o.ItemID == orderId);
        if (order is null) return;

        // Idempotency guard — skip if already cancelled
        if (order.Status == OrderStatus.Cancelled) return;

        // Cancel the order — UpdateOrderStatusAsync will also cancel the ShipRelay shipment.
        // systemOverride=true because the order may be in any status when payment expires (race with ShipRelay).
        await _orderService.UpdateOrderStatusAsync(orderId, new OrderUpdateStatusDTO
        {
            NewStatus = OrderStatus.Cancelled,
            NewPaymentStatus = PaymentStatus.Failed,
            Comment = reason
        }, systemOverride: true);

        // Enqueue cancellation notification email
        await EnqueueOrderCancelledAsync(order.CustomerEmail, order.CustomerName, order.OrderCode, reason);

        await _eventLogService.LogInformation("STRIPE_WEBHOOK", "PAYMENT_EXPIRED",
            $"Order {order.OrderCode} payment failed — order cancelled", $"Reason: {reason}");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task EnqueueOrderCancelledAsync(string email, string name, string orderCode, string reason)
    {
        try
        {
            var config = await _emailService.GetConfigAsync();
            await _emailService.SendEmailAsync(new EmailMessage
            {
                EmailFrom = config?.FromEmail ?? "noreply@lumnails.com",
                EmailTo = [email],
                EmailSubject = $"Order Cancelled — {orderCode}",
                EmailBody = $"Hi {name},<br><br>Unfortunately your order <strong>{orderCode}</strong> has been cancelled because the payment could not be completed.<br>Reason: {reason}<br><br>If you believe this is a mistake, please contact us or try placing a new order."
            });
        }
        catch (Exception ex)
        {
            await _eventLogService.LogException("STRIPE_WEBHOOK", "EMAIL_ENQUEUE_FAILED",
                $"Failed to enqueue cancellation email for order {orderCode}", ex);
        }
    }

    private async Task EnqueueOrderConfirmationAsync(string email, string name, string orderCode, decimal total)
    {
        try
        {
            var config = await _emailService.GetConfigAsync();
            await _emailService.SendEmailAsync(new EmailMessage
            {
                EmailFrom = config?.FromEmail ?? "noreply@lumnails.com",
                EmailTo = [email],
                EmailSubject = $"Order Confirmed — {orderCode}",
                EmailBody = $"Hi {name},<br><br>Your order <strong>{orderCode}</strong> has been confirmed and payment received.<br>Total: <strong>${total:F2}</strong><br><br>Thank you for shopping with LUM Nails!"
            });
        }
        catch (Exception ex)
        {
            await _eventLogService.LogException("STRIPE_WEBHOOK", "EMAIL_ENQUEUE_FAILED",
                $"Failed to enqueue confirmation email for order {orderCode}", ex);
        }
    }
}
