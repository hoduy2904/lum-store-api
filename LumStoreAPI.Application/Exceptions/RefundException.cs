namespace LumStoreAPI.Application.Exceptions;

/// <summary>
/// Thrown when a return cannot be refunded (invalid amount or rejected by Stripe).
/// Nothing has been persisted — the message is safe to show to the admin.
/// </summary>
public class RefundException : Exception
{
    public RefundException(string message) : base(message)
    {

    }

    public RefundException(string message, Exception innerException) : base(message, innerException)
    {

    }
}
