using LumStoreAPI.Core.Models.Enums;

namespace LumStoreAPI.Application.DTOs.OrderDTO;

public class OrderHistoryGetDTO
{
    public int HistoryId { get; set; }
    public OrderStatus? FromStatus { get; set; }
    public string? FromStatusName => FromStatus?.ToString();
    public OrderStatus ToStatus { get; set; }
    public string ToStatusName => ToStatus.ToString();
    public string? Comment { get; set; }
    public string? ChangedByName { get; set; }
    public bool IsSystemAction { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
