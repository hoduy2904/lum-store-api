using LumStoreAPI.Application.DTOs.UserDTO;
using LumStoreAPI.Core.Entities.Systems;
using LumStoreAPI.Core.Models.Enums;

namespace LumStoreAPI.Application.Interfaces
{
    public interface IUserService
    {
        Task<UserDTO?> GetCurrentUserAsync();
        Task<AccountStatus?> CheckAccountStatusAsync(int userID);
    }
}
