using LumStoreAPI.Application.DTOs.CustomerDTO;
using LumStoreAPI.Application.DTOs.OrderDTO;
using LumStoreAPI.Application.DTOs.Responses;
using LumStoreAPI.Application.Interfaces;
using LumStoreAPI.Core.Entities.Customers;
using LumStoreAPI.Core.Interfaces.Sytems;
using LumStoreAPI.Core.Models.Enums;
using LumStoreAPI.Infrastructure.Extensions;
using LumStoreAPI.Infrastructure.Repositories.Interfaces;

namespace LumStoreAPI.Application.Services;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customerRepo;
    private readonly IOrderService _orderService;
    private readonly IEventLogService _eventLog;

    public CustomerService(ICustomerRepository customerRepo, IOrderService orderService, IEventLogService eventLog)
    {
        _customerRepo = customerRepo;
        _orderService = orderService;
        _eventLog = eventLog;
    }

    public async Task<PagedResponse<CustomerGetDTO>> GetCustomersAsync(CustomerListRequest request)
    {
        var paged = await _customerRepo.GetProfilesAsync(
            request.Page, request.PageSize,
            request.Search, request.TierLevel,
            request.SortBy, request.Descending);

        var pagedDtos = paged.Select(MapToDTO).ToList().AsPagedEnumerable(paged.TotalRecords);
        return PagedResponse<CustomerGetDTO>.Success(pagedDtos, request.Page, request.PageSize);
    }

    public async Task<CustomerGetDTO?> GetCustomerAsync(int profileId)
    {
        var profile = await _customerRepo.GetProfileAsync(profileId);
        return profile == null ? null : MapToDTO(profile);
    }

    public async Task<CustomerGetDTO?> GetCustomerByUserIdAsync(int userId)
    {
        var profile = await _customerRepo.GetProfileByUserIdAsync(userId);
        return profile == null ? null : MapToDTO(profile);
    }

    public async Task<CustomerGetDTO> GetOrCreateProfileAsync(int userId)
    {
        var profile = await _customerRepo.GetProfileByUserIdAsync(userId);
        if (profile != null) return MapToDTO(profile);

        var created = await _customerRepo.InsertProfileAsync(new CustomerProfile
        {
            UserId = userId,
            TierLevel = CustomerTierLevel.Standard
        });
        return MapToDTO(created);
    }

    public async Task<CustomerGetDTO> UpdateProfileAsync(int profileId, CustomerUpdateDTO dto)
    {
        // Update User's display name if provided
        var profile = await _customerRepo.GetProfileAsync(profileId)
            ?? throw new KeyNotFoundException($"CustomerProfile {profileId} not found");

        if (dto.Name != null)
            await _customerRepo.UpdateUserNameAsync(profile.UserId, dto.Name);

        var updated = await _customerRepo.UpdateProfileAsync(profileId, p =>
        {
            if (dto.Phone != null) p.Phone = dto.Phone;
            if (dto.Birthday.HasValue) p.Birthday = dto.Birthday;
            if (dto.TierLevel.HasValue) p.TierLevel = dto.TierLevel.Value;
        });
        return MapToDTO(updated);
    }

    // ── Notes ─────────────────────────────────────────────────────────────

    public async Task<IEnumerable<CustomerNoteGetDTO>> GetCustomerNotesAsync(int profileId)
    {
        var notes = await _customerRepo.GetCustomerNotesAsync(profileId);
        return notes.Select(n => new CustomerNoteGetDTO
        {
            NoteId = n.ItemID,
            Note = n.Note,
            AuthorName = n.AuthorName,
            CreatedAt = n.CreatedAt
        });
    }

    public async Task<CustomerNoteGetDTO> AddCustomerNoteAsync(int profileId, CustomerNoteCreateDTO dto, int authorId, string authorName)
    {
        var note = await _customerRepo.InsertCustomerNoteAsync(new CustomerNote
        {
            CustomerProfileId = profileId,
            Note = dto.Note,
            AuthorId = authorId,
            AuthorName = authorName
        });
        return new CustomerNoteGetDTO
        {
            NoteId = note.ItemID,
            Note = note.Note,
            AuthorName = note.AuthorName,
            CreatedAt = note.CreatedAt
        };
    }

    public Task<bool> DeleteCustomerNoteAsync(int noteId)
        => _customerRepo.DeleteCustomerNoteAsync(noteId);

    // ── Loyalty Points ────────────────────────────────────────────────────

    public async Task<IEnumerable<LoyaltyPointGetDTO>> GetLoyaltyPointsAsync(int profileId)
    {
        var points = await _customerRepo.GetLoyaltyPointsAsync(profileId);
        return points.Select(p => new LoyaltyPointGetDTO
        {
            PointId = p.ItemID,
            Points = p.Points,
            Description = p.Description,
            OrderId = p.OrderId,
            ExpiresAt = p.ExpiresAt,
            CreatedAt = p.CreatedAt
        });
    }

    public async Task AwardPointsAsync(int profileId, int points, string description, int? orderId = null)
    {
        await _customerRepo.InsertLoyaltyPointAsync(new LoyaltyPoint
        {
            CustomerProfileId = profileId,
            Points = points,
            Description = description,
            OrderId = orderId
        });

        await _customerRepo.UpdateProfileAsync(profileId, p =>
        {
            p.TotalPoints += points;
            p.AvailablePoints += points;
        });

        await RecalculateTierAsync(profileId);
    }

    public async Task RedeemPointsAsync(int profileId, int points, string description)
    {
        var profile = await _customerRepo.GetProfileAsync(profileId)
            ?? throw new KeyNotFoundException($"CustomerProfile {profileId} not found");

        if (profile.AvailablePoints < points)
            throw new InvalidOperationException("Insufficient points to redeem");

        await _customerRepo.InsertLoyaltyPointAsync(new LoyaltyPoint
        {
            CustomerProfileId = profileId,
            Points = -points,
            Description = description
        });

        await _customerRepo.UpdateProfileAsync(profileId, p =>
        {
            p.AvailablePoints -= points;
        });
    }

    public async Task RecalculateTierAsync(int profileId)
    {
        var profile = await _customerRepo.GetProfileAsync(profileId);
        if (profile == null) return;

        var tiers = (await _customerRepo.GetTiersAsync())
            .Where(t => t.IsActive)
            .OrderByDescending(t => t.MinPoints)
            .ToList();

        var newTier = tiers.FirstOrDefault(t => profile.TotalPoints >= t.MinPoints)?.TierLevel
                      ?? CustomerTierLevel.Standard;

        if (profile.TierLevel != newTier)
        {
            await _customerRepo.UpdateProfileAsync(profileId, p => p.TierLevel = newTier);
            await _eventLog.LogInformation("CustomerService", "TIER_UPGRADED",
                $"Customer profile {profileId}: tier changed to {newTier}");
        }
    }

    // ── Tiers ─────────────────────────────────────────────────────────────

    private static readonly CustomerTierGetDTO[] _defaultTiers =
    [
        new() { TierLevel = CustomerTierLevel.Standard, TierName = "Standard", MinPoints = 0,     IsActive = true },
        new() { TierLevel = CustomerTierLevel.Silver,   TierName = "Silver",   MinPoints = 1000,  IsActive = true },
        new() { TierLevel = CustomerTierLevel.Gold,     TierName = "Gold",     MinPoints = 5000,  IsActive = true },
        new() { TierLevel = CustomerTierLevel.VIP,      TierName = "VIP",      MinPoints = 20000, IsActive = true },
    ];

    public async Task<IEnumerable<CustomerTierGetDTO>> GetTiersAsync()
    {
        var tiers = (await _customerRepo.GetTiersAsync()).ToList();
        return tiers.Count == 0 ? _defaultTiers : tiers.Select(MapTierToDTO);
    }

    public async Task<IEnumerable<CustomerTierGetDTO>> UpsertTiersAsync(IEnumerable<CustomerTierUpsertDTO> dtos)
    {
        var result = new List<CustomerTierGetDTO>();
        foreach (var dto in dtos)
            result.Add(await UpsertTierAsync(dto));
        return result;
    }

    public async Task<CustomerTierGetDTO> UpsertTierAsync(CustomerTierUpsertDTO dto)
    {
        var tier = new CustomerTier
        {
            TierLevel = dto.TierLevel,
            TierName = dto.TierName,
            MinPoints = dto.MinPoints,
            MaxPoints = dto.MaxPoints,
            DiscountPercent = dto.DiscountPercent,
            PointsPerDollar = dto.PointsPerDollar,
            BadgeColor = dto.BadgeColor,
            Description = dto.Description,
            IsActive = dto.IsActive
        };
        var saved = await _customerRepo.UpsertTierAsync(tier);
        return MapTierToDTO(saved);
    }

    // ── Stats ─────────────────────────────────────────────────────────────

    public async Task<CustomerStatsDTO> GetStatsAsync()
    {
        var total = await _customerRepo.CountCustomersAsync();
        var newCount = await _customerRepo.CountNewCustomersAsync(30);
        var activeCount = await _customerRepo.CountActiveCustomersAsync(90);
        var dist = await _customerRepo.GetTierDistributionAsync();

        return new CustomerStatsDTO
        {
            TotalCustomers = total,
            NewCustomers = newCount,
            ActiveCustomers = activeCount,
            TierBreakdown = new TierBreakdownDTO
            {
                Standard = dist.GetValueOrDefault(CustomerTierLevel.Standard),
                Silver   = dist.GetValueOrDefault(CustomerTierLevel.Silver),
                Gold     = dist.GetValueOrDefault(CustomerTierLevel.Gold),
                VIP      = dist.GetValueOrDefault(CustomerTierLevel.VIP)
            }
        };
    }

    public Task<PagedResponse<OrderGetDTO>> GetCustomerOrdersAsync(int profileId, int page, int pageSize)
        => _orderService.GetOrdersAsync(new OrderListRequest
        {
            Page = page,
            PageSize = pageSize,
            CustomerId = profileId
        });

    // ── Mappers ───────────────────────────────────────────────────────────

    private static CustomerGetDTO MapToDTO(CustomerProfile p) => new()
    {
        UserId = p.UserId,
        ProfileId = p.ItemID,
        FullName = p.User != null
            ? $"{p.User.FirstName} {p.User.MiddleName} {p.User.LastName}".Trim()
            : "",
        Email = p.User?.Email ?? "",
        Phone = p.Phone,
        Avatar = p.User?.Avatar,
        TierLevel = p.TierLevel,
        TotalPoints = p.TotalPoints,
        AvailablePoints = p.AvailablePoints,
        TotalSpent = p.TotalSpent,
        TotalOrders = p.TotalOrders,
        CreatedAt = p.CreatedAt,
        Birthday = p.Birthday
    };

    private static CustomerTierGetDTO MapTierToDTO(CustomerTier t) => new()
    {
        TierId = t.ItemID,
        TierLevel = t.TierLevel,
        TierName = t.TierName,
        MinPoints = t.MinPoints,
        MaxPoints = t.MaxPoints,
        DiscountPercent = t.DiscountPercent,
        PointsPerDollar = t.PointsPerDollar,
        BadgeColor = t.BadgeColor,
        Description = t.Description,
        IsActive = t.IsActive
    };
}
