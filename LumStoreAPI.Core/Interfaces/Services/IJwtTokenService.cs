using LumStoreAPI.Core.Entities.Systems;
using System.Security.Claims;

namespace LumStoreAPI.Core.Interfaces.Services
{
    public interface IJwtTokenService
    {
        public string GenerateAccessToken(User user, bool isRemember = false);
        ClaimsPrincipal? ValidateExpiredToken(string token);
        string GenerateRefreshToken();
        string GetRefreshToken(int userID);
    }
}
