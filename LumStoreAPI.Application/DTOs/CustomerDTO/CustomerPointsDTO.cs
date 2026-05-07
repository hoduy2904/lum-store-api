using System.ComponentModel.DataAnnotations;

namespace LumStoreAPI.Application.DTOs.CustomerDTO;

public class AwardPointsDTO
{
    [Range(1, int.MaxValue, ErrorMessage = "Points must be greater than 0")]
    public int Points { get; set; }

    // Spec field name for award is "reason"
    [Required, MaxLength(500)]
    public string Description { get; set; } = default!;

    // Accept "reason" as an alias from the JSON body
    public string? Reason { set { if (!string.IsNullOrEmpty(value)) Description = value; } }
}

public class RedeemPointsDTO
{
    [Range(1, int.MaxValue, ErrorMessage = "Points must be greater than 0")]
    public int Points { get; set; }

    [Required, MaxLength(500)]
    public string Description { get; set; } = default!;
}
