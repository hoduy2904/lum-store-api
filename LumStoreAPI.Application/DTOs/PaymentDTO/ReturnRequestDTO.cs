namespace LumStoreAPI.Application.DTOs.PaymentDTO
{
    public record ReturnRequestDTO(
        string OrderCode,
        string PaymentIntentId,
        string? Reason = default);
}
