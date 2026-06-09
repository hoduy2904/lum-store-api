namespace LumStoreAPI.Application.Interfaces;

public interface IStripeWebhookService
{
    /// <summary>
    /// Validates the Stripe-Signature header value against the configured webhook secret.
    /// Returns false if the secret is not configured or the signature is invalid.
    /// </summary>
    Task<bool> ValidateSignatureAsync(string json, string signature);

    /// <summary>
    /// Processes a raw Stripe webhook event payload.
    /// Handles: checkout.session.completed (payment success) and checkout.session.expired (payment failed/cancelled).
    /// </summary>
    Task ProcessWebhookAsync(string json);
}
