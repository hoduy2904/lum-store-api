using LumStoreAPI.Application.DTOs.UserDTO;
using LumStoreAPI.Core.Interfaces.ContentEngine;
using LumStoreAPI.Core.Models.Enums;

namespace LumStoreAPI.Application.Interfaces
{
    public interface IUserService
    {
        Task<UserDTO?> GetCurrentUserAsync();
        Task<AccountStatus?> CheckAccountStatusAsync(int userID);
        Task<UserDTO?> GetUserAsync(int userId);
        Task<UserDTO?> CreateUserAsync(UserAdminCreateRequest request);
        Task<UserDTO?> UpdateUserAsync(int userId, UserAdminUpdateRequest request);
        Task<IPagedEnumerable<UserDTO>> GetUsersAsync (UserAdminRequest request);
    }
}