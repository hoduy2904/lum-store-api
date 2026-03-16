using LumStoreAPI.Application.DTOs.AuthDTO;
using LumStoreAPI.Application.DTOs.Responses;
using LumStoreAPI.Application.DTOs.UserDTO;

namespace LumStoreAPI.Application.Interfaces
{
    public interface IAuthService
    {
        Task<APIResponse<TokenResponse>> AuthenticateAsync(AuthRequest request);
        Task<APIResponse<TokenResponse>> RefreshTokenAsync(TokenRequest request);
        Task LogoutAsync();
        Task<bool> IsValidCodeAsync(int userID, string code);
        Task<UserDTO> RegisterUserAsync(UserCreateRequest request);
        Task<APIResponseBase> VerifyCode(string code);
        Task<bool> ResendVerifyCodeAsync(int user);
    }
}
