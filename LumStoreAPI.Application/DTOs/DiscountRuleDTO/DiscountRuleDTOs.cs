using System.ComponentModel.DataAnnotations;

namespace LumStoreAPI.Application.DTOs.DiscountRuleDTO;

public class DiscountRuleGetDTO
{
    public int RuleId { get; set; }
    public int? ProductId { get; set; }
    public int? VariantId { get; set; }
    public string RuleName { get; set; } = default!;
    public int MinQuantity { get; set; }
    public int? MaxQuantity { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal? DiscountAmount { get; set; }
    public DateTimeOffset? StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
    public bool IsActive { get; set; }
}

public class DiscountRuleUpsertDTO
{
    public int? ProductId { get; set; }
    public int? VariantId { get; set; }

    [Required, MaxLength(200)]
    public string RuleName { get; set; } = default!;

    [Range(1, int.MaxValue)]
    public int MinQuantity { get; set; } = 1;

    public int? MaxQuantity { get; set; }

    [Range(0, 100)]
    public decimal DiscountPercent { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? DiscountAmount { get; set; }

    public DateTimeOffset? StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }

    public bool IsActive { get; set; } = true;
}
