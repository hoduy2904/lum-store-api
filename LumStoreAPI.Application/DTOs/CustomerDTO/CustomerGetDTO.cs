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
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
    public string? Country { get; set; }
    public CustomerTierLevel TierLevel { get; set; }
    public string TierName => TierLevel.ToString();
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
    public string? Search { get; set; }
    public CustomerTierLevel? TierLevel { get; set; }
    public string SortBy { get; set; } = "CreatedAt";
    public bool Descending { get; set; } = true;
}
