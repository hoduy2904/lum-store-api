using LumStoreAPI.Application.Interfaces;
using LumStoreAPI.Core.Entities.Systems;
using LumStoreAPI.Core.Interfaces.Repositories;
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
        public Task<User?> GetCurrentUserAsync()
        {
            if (!int.TryParse(_httpContextAccessor.HttpContext?.User.Claims.FirstOrDefault(x => x.Type.Equals("id"))?.Value, out int userId))
            {
                return Task.FromResult<User?>(null);
            }
            return _userRepository.GetUserAsync(userId);
        }
    }
}
