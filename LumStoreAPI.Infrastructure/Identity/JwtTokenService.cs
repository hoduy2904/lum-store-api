using LumStoreAPI.Core.Entities.Systems;
using LumStoreAPI.Core.Interfaces.Services;
using LumStoreAPI.Core.Models.Enums;
using LumStoreAPI.Core.Models.Systems;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace LumStoreAPI.Infrastructure.Identity
{
    internal class JwtTokenService : IJwtTokenService
    {
        public string GenerateAccessToken(User user, bool isRemember)
        {
            var key = AppConfiguration.JwtSettings.Key;

            var claims = new[]
            {
                new Claim("id", user.ItemID.ToString()),
                new Claim(JwtRegisteredClaimNames.Name, user.FullName),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, (user.IsLocked || !user.IsVerified) ? "pre" : user.IsAdmin ? nameof(UserRole.ADMIN) : nameof(UserRole.USER))
            };

            var symmetricSecurityKey = new SymmetricSecurityKey(AppConfiguration.JwtSettings.EncodingKey);
            var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(GetExpiresInMinutes(isRemember)),
                signingCredentials: signingCredentials);

            var tokenHandler = new JwtSecurityTokenHandler().WriteToken(token);
            return tokenHandler;
        }

        public string GenerateRefreshToken()
        {
            byte[] randomBytes = new byte[16];

            RandomNumberGenerator.Fill(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }

        public string GetRefreshToken(int userID)
        {
            return "";
        }

        public ClaimsPrincipal? ValidateExpiredToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                var claimsPrincipal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateLifetime = false,
                    IssuerSigningKey = new SymmetricSecurityKey(AppConfiguration.JwtSettings.EncodingKey),
                    ValidateIssuerSigningKey = true,

                }, out _);

                return claimsPrincipal;
            }
            catch
            {
                return null;
            }
        }

        private int GetExpiresInMinutes(bool isRemember)
        {
            return 15;
        }
    }
}
