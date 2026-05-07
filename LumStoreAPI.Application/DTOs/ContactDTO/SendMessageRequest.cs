using System.ComponentModel.DataAnnotations;

namespace LumStoreAPI.Application.DTOs.ContactDTO;

public class SendMessageRequest
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = default!;

    [Required]
    [EmailAddress]
    [MaxLength(200)]
    public string Email { get; set; } = default!;

    [Required]
    [MaxLength(200)]
    public string Subject { get; set; } = default!;

    [Required]
    [MaxLength(2000)]
    public string Message { get; set; } = default!;
}
