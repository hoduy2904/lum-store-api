using LumStoreAPI.Application.DTOs.AuthDTO;
using LumStoreAPI.Application.DTOs.Responses;
using LumStoreAPI.Application.Interfaces;
using LumStoreAPI.Core.Entities.Systems;
using LumStoreAPI.Core.Interfaces.Repositories;
using LumStoreAPI.Core.Interfaces.Services;
using LumStoreAPI.Core.Models.Constants.Systems;
using LumStoreAPI.Libraries.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace LumStoreAPI.Application.Services
{
    internal class SocialAuthService : ISocialAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IUserTokenRepository _userTokenRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public SocialAuthService(
            IUserRepository userRepository,
            IJwtTokenService jwtTokenService,
            IUserTokenRepository userTokenRepository,
            IHttpContextAccessor httpContextAccessor,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            _userRepository = userRepository;
            _jwtTokenService = jwtTokenService;
            _userTokenRepository = userTokenRepository;
            _httpContextAccessor = httpContextAccessor;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        DateTime RefreshTokenExpiry => DateTime.UtcNow.AddDays(7);

        CookieOptions CookieOptions => new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Path = "/"
        };

        public async Task<APIResponse<TokenResponse>> SocialLoginAsync(SocialLoginRequest request)
        {
            var provider = request.Provider.ToLowerInvariant();

            SocialUserInfo? userInfo = provider switch
            {
                "google" => await ValidateGoogleTokenAsync(request.IdToken),
                "facebook" => await ValidateFacebookTokenAsync(request.IdToken),
                _ => null
            };

            if (userInfo == null)
                return APIResponse<TokenResponse>.Failure("INVALID_TOKEN", ["Invalid or expired token from provider."]);

            // 1. Try find by provider ID
            var user = provider == "google"
                ? (await _userRepository.GetUsersAsync(q => q.Where(u => u.GoogleId == userInfo.ProviderId).Take(1))).FirstOrDefault()
                : (await _userRepository.GetUsersAsync(q => q.Where(u => u.FacebookId == userInfo.ProviderId).Take(1))).FirstOrDefault();

            // 2. Try find by email
            if (user == null && !string.IsNullOrEmpty(userInfo.Email))
            {
                user = (await _userRepository.GetUsersAsync(q => q.Where(u => u.Email == userInfo.Email).Take(1))).FirstOrDefault();
            }

            if (user == null)
            {
                // 3. Create new social user
                user = await CreateSocialUserAsync(userInfo, provider);
            }
            else
            {
                // 4. Link social account if not already set
                await LinkSocialAccountIfNeededAsync(user, userInfo, provider);
            }

            var accessToken = _jwtTokenService.GenerateAccessToken(user);
            var refreshToken = _jwtTokenService.GenerateRefreshToken();
            var securityToken = JwtTokenHelper.GetJwtSecurityToken(accessToken);

            await _userTokenRepository.InsertToken(new UserToken
            {
                RefreshToken = refreshToken,
                TokenID = Guid.Parse(securityToken.Id),
                UserID = user.ItemID,
                ValidTo = RefreshTokenExpiry
            });

            SetCookies(accessToken, refreshToken);

            return APIResponse<TokenResponse>.Success(new TokenResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            }, ["Authenticated"]);
        }

        private async Task<User> CreateSocialUserAsync(SocialUserInfo userInfo, string provider)
        {
            var (firstName, lastName) = SplitName(userInfo.Name);
            var email = userInfo.Email ?? $"{userInfo.ProviderId}@facebook.placeholder";

            var userName = $"{provider}_{userInfo.ProviderId}";
            if (userName.Length > 50) userName = userName[..50];

            var user = new User
            {
                UserName = userName,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                UserPassword = HashHelper.HashPassword(Guid.NewGuid().ToString()),
                IsEnabled = true,
                IsVerified = true,
                IsLocked = false,
                Avatar = userInfo.AvatarUrl,
                Provider = provider,
                GoogleId = provider == "google" ? userInfo.ProviderId : null,
                FacebookId = provider == "facebook" ? userInfo.ProviderId : null
            };

            return await _userRepository.InsertUserAsync(user);
        }

        private async Task LinkSocialAccountIfNeededAsync(User user, SocialUserInfo userInfo, string provider)
        {
            bool alreadyLinked = provider == "google"
                ? user.GoogleId == userInfo.ProviderId
                : user.FacebookId == userInfo.ProviderId;

            if (alreadyLinked) return;

            if (provider == "google")
            {
                await _userRepository.UpdateUsersAsync(
                    x => x.ItemID == user.ItemID,
                    x => x.Set(p => p.GoogleId, userInfo.ProviderId)
                           .Set(p => p.Provider, "google"));
            }
            else
            {
                await _userRepository.UpdateUsersAsync(
                    x => x.ItemID == user.ItemID,
                    x => x.Set(p => p.FacebookId, userInfo.ProviderId)
                           .Set(p => p.Provider, "facebook"));
            }
        }

        private async Task<SocialUserInfo?> ValidateGoogleTokenAsync(string idToken)
        {
            var client = _httpClientFactory.CreateClient("SocialAuth");
            HttpResponseMessage response;
            try
            {
                response = await client.GetAsync($"https://oauth2.googleapis.com/tokeninfo?id_token={idToken}");
            }
            catch
            {
                return null;
            }

            if (!response.IsSuccessStatusCode) return null;

            using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var root = doc.RootElement;

            if (root.TryGetProperty("error_description", out _)) return null;

            // Validate audience against configured ClientId
            var clientId = _configuration["Authentication:Google:ClientId"];
            if (!string.IsNullOrEmpty(clientId) && clientId != "YOUR_GOOGLE_CLIENT_ID")
            {
                if (!root.TryGetProperty("aud", out var audEl) || audEl.GetString() != clientId)
                    return null;
            }

            if (!root.TryGetProperty("sub", out var subEl)) return null;

            return new SocialUserInfo
            {
                ProviderId = subEl.GetString()!,
                Email = root.TryGetProperty("email", out var emailEl) ? emailEl.GetString() : null,
                Name = root.TryGetProperty("name", out var nameEl) ? nameEl.GetString() : null,
                AvatarUrl = root.TryGetProperty("picture", out var picEl) ? picEl.GetString() : null
            };
        }

        private async Task<SocialUserInfo?> ValidateFacebookTokenAsync(string accessToken)
        {
            var client = _httpClientFactory.CreateClient("SocialAuth");
            HttpResponseMessage response;
            try
            {
                response = await client.GetAsync($"https://graph.facebook.com/me?fields=id,name,email,picture&access_token={accessToken}");
            }
            catch
            {
                return null;
            }

            if (!response.IsSuccessStatusCode) return null;

            using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var root = doc.RootElement;

            if (root.TryGetProperty("error", out _)) return null;
            if (!root.TryGetProperty("id", out var idEl)) return null;

            string? avatarUrl = null;
            if (root.TryGetProperty("picture", out var picEl)
                && picEl.TryGetProperty("data", out var picData)
                && picData.TryGetProperty("url", out var urlEl))
            {
                avatarUrl = urlEl.GetString();
            }

            return new SocialUserInfo
            {
                ProviderId = idEl.GetString()!,
                Email = root.TryGetProperty("email", out var emailEl) ? emailEl.GetString() : null,
                Name = root.TryGetProperty("name", out var nameEl) ? nameEl.GetString() : null,
                AvatarUrl = avatarUrl
            };
        }

        private static (string firstName, string lastName) SplitName(string? fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName)) return ("User", "Unknown");
            var parts = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1) return (parts[0], "User");
            return (parts[0], string.Join(" ", parts[1..]));
        }

        private void SetCookies(string accessToken, string refreshToken)
        {
            var accessOpts = CookieOptions;
            accessOpts.Expires = RefreshTokenExpiry;
            _httpContextAccessor.HttpContext?.Response.Cookies.Append(AuthSystemConstants.ACCESS_TOKEN_COOKIE_NAME, accessToken, accessOpts);

            var refreshOpts = CookieOptions;
            refreshOpts.Expires = RefreshTokenExpiry;
            _httpContextAccessor.HttpContext?.Response.Cookies.Append(AuthSystemConstants.REFRESH_TOKEN_COOKIE_NAME, refreshToken, refreshOpts);
        }
    }

    internal class SocialUserInfo
    {
        public string ProviderId { get; set; } = default!;
        public string? Email { get; set; }
        public string? Name { get; set; }
        public string? AvatarUrl { get; set; }
    }
}
