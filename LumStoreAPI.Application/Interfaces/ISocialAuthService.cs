using LumStoreAPI.Application.DTOs.AuthDTO;
using LumStoreAPI.Application.DTOs.Responses;

namespace LumStoreAPI.Application.Interfaces
{
    public interface ISocialAuthService
    {
        Task<APIResponse<TokenResponse>> SocialLoginAsync(SocialLoginRequest request);
    }
}
