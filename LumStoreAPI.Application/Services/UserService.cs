using LumStoreAPI.Application.DTOs.UserDTO;
using LumStoreAPI.Application.Interfaces;
using LumStoreAPI.Core.Entities.Systems;
using LumStoreAPI.Core.Interfaces.Repositories;
using LumStoreAPI.Core.Models.Enums;
using Microsoft.AspNetCore.Http;

namespace LumStoreAPI.Application.Services
{
    internal class UserService : IUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserRepository _userRepository;
        public UserService(IHttpContextAccessor httpContextAccessor, IUserRepository userRepository)
        {
            _httpContextAccessor = httpContextAccessor;
            _userRepository = userRepository;
        }

        public async Task<AccountStatus?> CheckAccountStatusAsync(int userID)
        {
            var user = await _userRepository.GetUserAsync(userID);
            if (user == null || !user.IsEnabled)
                return null;
            return user.IsLocked ? AccountStatus.LOCKED : !user.IsVerified ? AccountStatus.INACTIVE : AccountStatus.ACTIVE;
        }

        public async Task<UserDTO?> GetCurrentUserAsync()
        {
            if (!int.TryParse(_httpContextAccessor.HttpContext?.User.Claims.FirstOrDefault(x => x.Type.Equals("id"))?.Value, out int userId))
            {
                return null;
            }
            var user = await _userRepository.GetUserAsync(userId);

            return user == null ? null : new UserDTO(user);
        }
    }
}
