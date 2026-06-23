using LumStoreAPI.Application.DTOs.UserDTO;
using LumStoreAPI.Application.Interfaces;
using LumStoreAPI.Core.Interfaces.ContentEngine;
using LumStoreAPI.Core.Interfaces.Repositories;
using LumStoreAPI.Core.Models.Enums;
using LumStoreAPI.Libraries.Helpers;
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

        public async Task<UserDTO?> GetUserAsync(int userId)
        {
            var user= await _userRepository.GetUserAsync(userId);
            if (user == null) return null;
            return new UserDTO(user);
        }

        public async Task<UserDTO?> CreateUserAsync(UserAdminCreateRequest request)
        {
           var newUser = await _userRepository.InsertUserAsync(request.GetEntity);
           return new UserDTO(newUser);
        }

        public async Task<UserDTO?> UpdateUserAsync(int userId, UserAdminUpdateRequest request)
        {
           var user = await _userRepository.UpdateUserAsync(userId, user =>
            {
                user.IsLocked = request.IsLocked;
                user.IsVerified = request.IsVerified;
                user.Email = request.Email;
                user.FirstName = request.FirstName;
                user.LastName = request.LastName;
                user.Avatar = request.Avatar;
                user.Role = request.UserRole;
                user.MiddleName = request.MiddleName;
                user.IsEnabled = request.IsEnabled;
                if (!string.IsNullOrWhiteSpace(request.Password))
                {
                    user.UserPassword = HashHelper.HashPassword(request.Password);
                }
            });
           
           return new UserDTO(user);
        }

        public async Task<IPagedEnumerable<UserDTO>> GetUsersAsync(UserAdminRequest request)
        {
            var users = await _userRepository.GetUsersAsync(request.Page, request.PageSize, 
                query=>
                    query.Where(x=> (request.UserRole== null || x.Role == request.UserRole) &&
                           (string.IsNullOrWhiteSpace(request.Search) || x.UserName.Contains(request.Search!) || x.Email.Contains(request.Search!))));
            
            var userDto = users.Select(x=> new UserDTO(x));
            return userDto;
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
