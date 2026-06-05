using System.ComponentModel.DataAnnotations;

namespace LumStoreAPI.Application.DTOs.DiscountRuleDTO;

public class DiscountRuleGetDTO
{
    public int RuleId { get; set; }
    public int[] ProductIds { get; set; } = [];
    public string RuleName { get; set; } = default!;
    public int MinQuantity { get; set; }
    public int? MaxQuantity { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal DiscountAmount { get; set; }
    public DateTimeOffset? StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
    public bool IsActive { get; set; }
}

public class DiscountRuleUpsertDTO : IValidatableObject
{
    public int[] ProductIds { get; set; } = [];

    [Required, MaxLength(200)]
    public string RuleName { get; set; } = default!;

    [Range(1, int.MaxValue)]
    public int MinQuantity { get; set; } = 1;

    public int? MaxQuantity { get; set; }

    [Range(0, 100, ErrorMessage = "DiscountPercent must be between 0 and 100.")]
    public decimal DiscountPercent { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "DiscountAmount must be >= 0.")]
    public decimal DiscountAmount { get; set; }

    public DateTimeOffset? StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }

    public bool IsActive { get; set; } = true;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (DiscountPercent <= 0 && DiscountAmount <= 0)
            yield return new ValidationResult(
                "At least one of DiscountPercent or DiscountAmount must be greater than 0.",
                [nameof(DiscountPercent), nameof(DiscountAmount)]);
    }
}
