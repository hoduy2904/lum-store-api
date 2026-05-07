using System.ComponentModel.DataAnnotations;

namespace LumStoreAPI.Application.DTOs.AddressDTO;

public class AddressRequest
{
    [Required(ErrorMessage = "Phone is required.")]
    [MaxLength(30)]
    public string Phone { get; set; } = default!;

    [Required(ErrorMessage = "Address is required.")]
    [MaxLength(300)]
    public string Address { get; set; } = default!;

    [Required(ErrorMessage = "City is required.")]
    [MaxLength(100)]
    public string City { get; set; } = default!;

    [Required(ErrorMessage = "State is required.")]
    [MaxLength(100)]
    public string State { get; set; } = default!;

    [Required(ErrorMessage = "ZipCode is required.")]
    [MaxLength(20)]
    public string ZipCode { get; set; } = default!;

    [MaxLength(500)]
    public string? Details { get; set; }

    public bool IsDefault { get; set; }
}
