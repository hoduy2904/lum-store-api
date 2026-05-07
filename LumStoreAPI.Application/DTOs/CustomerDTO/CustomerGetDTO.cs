using System.Text.Json.Serialization;
using LumStoreAPI.Core.Models.Enums;

namespace LumStoreAPI.Application.DTOs.CustomerDTO;

public class CustomerGetDTO
{
    public int UserId { get; set; }
    public int ProfileId { get; set; }
    public string FullName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string? Phone { get; set; }
    public string? Avatar { get; set; }
    [JsonIgnore]
    public CustomerTierLevel TierLevel { get; set; }
    public string? TierName => Enum.GetName(TierLevel);
    public int TotalPoints { get; set; }
    public int AvailablePoints { get; set; }
    public decimal TotalSpent { get; set; }
    public int TotalOrders { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? Birthday { get; set; }
}

public class CustomerListRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;

    // "limit" is the spec's query param alias for pageSize
    public int? Limit { get => null; set { if (value.HasValue) PageSize = value.Value; } }

    public string? Search { get; set; }
    public CustomerTierLevel? TierLevel { get; set; }

    // "tier" as string ("all" = no filter); maps into TierLevel
    public string? Tier
    {
        get => null;
        set
        {
            if (string.IsNullOrEmpty(value) || value.Equals("all", StringComparison.OrdinalIgnoreCase))
                TierLevel = null;
            else if (Enum.TryParse<CustomerTierLevel>(value, ignoreCase: true, out var t))
                TierLevel = t;
        }
    }

    public string SortBy { get; set; } = "CreatedAt";
    public bool Descending { get; set; } = true;
}
