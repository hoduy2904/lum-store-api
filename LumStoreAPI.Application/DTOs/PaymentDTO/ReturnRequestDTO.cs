namespace LumStoreAPI.Application.DTOs.PaymentDTO
{
    public record ReturnRequestDTO(
        int RefundId,
        string OrderCode,
        string PaymentIntentId,
        string? Reason = default);
}
