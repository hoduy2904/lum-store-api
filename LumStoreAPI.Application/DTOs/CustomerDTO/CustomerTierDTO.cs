using LumStoreAPI.Core.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace LumStoreAPI.Application.DTOs.CustomerDTO;

public class CustomerTierGetDTO
{
    public int TierId { get; set; }
    public CustomerTierLevel TierLevel { get; set; }
    public string TierName { get; set; } = default!;
    public int MinPoints { get; set; }
    public int MaxPoints { get; set; }
    public decimal DiscountPercent { get; set; }
    public int PointsPerDollar { get; set; }
    public string? BadgeColor { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}

public class CustomerTierUpsertDTO
{
    [Required]
    public CustomerTierLevel TierLevel { get; set; }

    [Required, MaxLength(50)]
    public string TierName { get; set; } = default!;

    [Range(0, int.MaxValue)]
    public int MinPoints { get; set; }

    [Range(0, int.MaxValue)]
    public int MaxPoints { get; set; }

    [Range(0, 100)]
    public decimal DiscountPercent { get; set; }

    [Range(0, int.MaxValue)]
    public int PointsPerDollar { get; set; } = 1;

    [MaxLength(20)]
    public string? BadgeColor { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
}
