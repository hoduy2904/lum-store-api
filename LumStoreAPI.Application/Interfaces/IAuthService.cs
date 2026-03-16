using LumStoreAPI.Application.DTOs.AuthDTO;
using LumStoreAPI.Application.DTOs.Responses;

namespace LumStoreAPI.Application.Interfaces
{
    public interface IAuthService
    {
        Task<APIResponse<TokenResponse>> AuthenticateAsync(AuthRequest request);
        Task<APIResponse<TokenResponse>> RefreshTokenAsync(TokenRequest request);
        Task LogoutAsync();
        Task<bool> IsValidCodeAsync(int userID, string code);
    }
}
