using LumStoreAPI.Core.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace LumStoreAPI.Application.DTOs.OrderDTO;

public class OrderUpdateStatusDTO
{
    [Required]
    public OrderStatus NewStatus { get; set; }

    [MaxLength(500)]
    public string? Comment { get; set; }
}
