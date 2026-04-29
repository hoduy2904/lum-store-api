using System.ComponentModel.DataAnnotations;

namespace LumStoreAPI.Application.DTOs.CustomerDTO;

public class CustomerUpdateDTO
{
    [MaxLength(30)] public string? Phone { get; set; }
    [MaxLength(500)] public string? Address { get; set; }
    [MaxLength(100)] public string? City { get; set; }
    [MaxLength(100)] public string? State { get; set; }
    [MaxLength(20)] public string? ZipCode { get; set; }
    [MaxLength(10)] public string? Country { get; set; }
    public DateTimeOffset? Birthday { get; set; }
}

public class LoyaltyPointGetDTO
{
    public int PointId { get; set; }
    public int Points { get; set; }
    public string Description { get; set; } = default!;
    public int? OrderId { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public class CustomerStatsDTO
{
    public int TotalCustomers { get; set; }
    public int StandardCustomers { get; set; }
    public int SilverCustomers { get; set; }
    public int GoldCustomers { get; set; }
    public int VipCustomers { get; set; }
}

public class StoreLoyaltyDTO
{
    public int TotalPoints { get; set; }
    public int AvailablePoints { get; set; }
    public string? TierLevel { get; set; }
    public IEnumerable<LoyaltyPointGetDTO> History { get; set; } = [];
}
