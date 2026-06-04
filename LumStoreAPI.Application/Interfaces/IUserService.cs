using LumStoreAPI.Application.DTOs.UserDTO;
using LumStoreAPI.Core.Models.Enums;

namespace LumStoreAPI.Application.Interfaces
{
    public interface IUserService
    {
        Task<UserDTO?> GetCurrentUserAsync();
        Task<AccountStatus?> CheckAccountStatusAsync(int userID);
        int GetCurrentUserId();
    }
}
