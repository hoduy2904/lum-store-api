using System.ComponentModel.DataAnnotations;

namespace LumStoreAPI.Application.DTOs.StoreOrderDTO;

public class StorePlaceOrderRequest
{
    [Required]
    public int AddressId { get; set; }

    [MaxLength(1000)]
    public string? Note { get; set; }
}
