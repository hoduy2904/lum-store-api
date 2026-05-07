using LumStoreAPI.Core.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace LumStoreAPI.Application.DTOs.CustomerDTO;

public class CustomerUpdateDTO
{
    /// <summary>Full display name — updates User.FirstName / MiddleName / LastName.</summary>
    [MaxLength(200)] public string? Name { get; set; }

    [MaxLength(30)] public string? Phone { get; set; }
    [MaxLength(500)] public string? Address { get; set; }
    [MaxLength(100)] public string? City { get; set; }
    [MaxLength(100)] public string? State { get; set; }
    [MaxLength(20)] public string? ZipCode { get; set; }
    [MaxLength(10)] public string? Country { get; set; }
    public DateTimeOffset? Birthday { get; set; }

    /// <summary>Manual tier override by admin.</summary>
    public CustomerTierLevel? TierLevel { get; set; }
}

public class LoyaltyPointGetDTO
{
    [JsonPropertyName("id")]
    public int PointId { get; set; }

    [JsonPropertyName("points")]
    public int Points { get; set; }

    // Spec calls this "reason"
    [JsonPropertyName("reason")]
    public string Description { get; set; } = default!;

    public int? OrderId { get; set; }

    [JsonPropertyName("expiresAt")]
    public DateTimeOffset? ExpiresAt { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTimeOffset CreatedAt { get; set; }
}

public class TierBreakdownDTO
{
    public int Standard { get; set; }
    public int Silver { get; set; }
    public int Gold { get; set; }
    public int VIP { get; set; }
}

public class CustomerStatsDTO
{
    public int TotalCustomers { get; set; }
    /// <summary>Customers registered in the last 30 days.</summary>
    public int NewCustomers { get; set; }
    /// <summary>Customers with at least one order in the last 90 days.</summary>
    public int ActiveCustomers { get; set; }
    public TierBreakdownDTO TierBreakdown { get; set; } = new();
}

public class StoreLoyaltyDTO
{
    public int TotalPoints { get; set; }
    public int AvailablePoints { get; set; }
    public string? TierLevel { get; set; }
    public IEnumerable<LoyaltyPointGetDTO> History { get; set; } = [];
}
