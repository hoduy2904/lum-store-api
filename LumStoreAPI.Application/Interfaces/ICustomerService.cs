using LumStoreAPI.Application.DTOs.CustomerDTO;
using LumStoreAPI.Application.DTOs.OrderDTO;
using LumStoreAPI.Application.DTOs.Responses;
using LumStoreAPI.Core.Models.Enums;

namespace LumStoreAPI.Application.Interfaces;

public interface ICustomerService
{
    Task<PagedResponse<CustomerGetDTO>> GetCustomersAsync(CustomerListRequest request);
    Task<CustomerGetDTO?> GetCustomerAsync(int profileId);
    Task<CustomerGetDTO?> GetCustomerByUserIdAsync(int userId);
    Task<CustomerGetDTO> GetOrCreateProfileAsync(int userId);
    Task<CustomerGetDTO> UpdateProfileAsync(int profileId, CustomerUpdateDTO dto);

    // ── Notes ─────────────────────────────────────────────────────────────
    Task<IEnumerable<CustomerNoteGetDTO>> GetCustomerNotesAsync(int profileId);
    Task<CustomerNoteGetDTO> AddCustomerNoteAsync(int profileId, CustomerNoteCreateDTO dto, int authorId, string authorName);
    Task<bool> DeleteCustomerNoteAsync(int noteId);

    // ── Loyalty Points ────────────────────────────────────────────────────
    Task<IEnumerable<LoyaltyPointGetDTO>> GetLoyaltyPointsAsync(int profileId);
    Task AwardPointsAsync(int profileId, int points, string description, int? orderId = null);
    Task RevokeOrderPointsAsync(int profileId, int orderId, string description);
    Task RedeemPointsAsync(int profileId, int points, string description);
    Task RecalculateTierAsync(int profileId);

    // ── Tiers ─────────────────────────────────────────────────────────────
    Task<IEnumerable<CustomerTierGetDTO>> GetTiersAsync();
    Task<CustomerTierGetDTO> UpsertTierAsync(CustomerTierUpsertDTO dto);
    Task<IEnumerable<CustomerTierGetDTO>> UpsertTiersAsync(IEnumerable<CustomerTierUpsertDTO> dtos);

    // ── Orders ────────────────────────────────────────────────────────────
    Task<PagedResponse<OrderGetDTO>> GetCustomerOrdersAsync(int profileId, int page, int pageSize);

    // ── Stats ─────────────────────────────────────────────────────────────
    Task<CustomerStatsDTO> GetStatsAsync();
}
