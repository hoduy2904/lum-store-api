using LumStoreAPI.Application.DTOs.AuthDTO;
using LumStoreAPI.Application.DTOs.Responses;
using LumStoreAPI.Application.Interfaces;
using LumStoreAPI.Core.Interfaces.Repositories;
using LumStoreAPI.Core.Interfaces.Services;
using LumStoreAPI.Libraries.Helpers;

namespace LumStoreAPI.Application.Services
{
    internal class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IUserTokenRepository _userTokenRepository;
        public AuthService(IUserRepository userRepository, IJwtTokenService jwtTokenService, IUserTokenRepository userTokenRepository)
        {
            _userRepository = userRepository;
            _jwtTokenService = jwtTokenService;
            _userTokenRepository = userTokenRepository;
        }
        public async Task<APIResponse<TokenResponse>> AuthenticateAsync(AuthRequest request)
        {
            var user = (await _userRepository.GetUsersAsync(query =>
            {
                query = query.Where(x => x.UserName.Equals(request.UserName) && x.IsEnabled).Take(1);
                return query;
            })).FirstOrDefault();

            if (user == null || !HashHelper.VerifyPassword(request.Password, user.UserPassword))
            {
                return APIResponse<TokenResponse>.Failure(["Invalid username or password"]);
            }

            if (user.IsLocked)
            {
                return APIResponse<TokenResponse>.Failure(["The account has been locked"]);
            }
            if (!user.IsVerified)
            {
                return APIResponse<TokenResponse>.Failure(["The account is inactive, please verify the account"]);
            }

            var accessToken = _jwtTokenService.GenerateAccessToken(user, request.IsRemember);

            var response = new TokenResponse()
            {
                AccessToken = accessToken,
                RefreshToken = _jwtTokenService.GenerateRefreshToken()
            };

            var securityToken = JwtTokenHelper.GetJwtSecurityToken(accessToken);

            await _userTokenRepository.InsertToken(new Core.Entities.Systems.UserToken
            {
                RefreshToken = response.RefreshToken,
                TokenID = Guid.Parse(securityToken.Id),
                UserID = user.ItemID,
                ValidTo = DateTime.UtcNow.AddDays(7)
            });

            return APIResponse<TokenResponse>.Success(response, ["Authenticated"]);

        }

        public Task<bool> IsValidCodeAsync(int userID, string code)
        {
            return _userRepository.CheckUserAsync(x => x.ItemID == userID && !string.IsNullOrEmpty(x.VerifyCode) && x.VerifyCode.Equals(code.Trim()));
        }

        public async Task<APIResponse<TokenResponse>> RefreshTokenAsync(TokenRequest request)
        {
            var tokenSecurity = JwtTokenHelper.GetJwtSecurityToken(request.accessToken);
            if (await _userTokenRepository.IsValidRefreshToken(Guid.Parse(tokenSecurity.Id), request.refreshToken))
            {
                if (!int.TryParse(tokenSecurity.Claims.FirstOrDefault(x => x.Type.Equals("id"))?.Value, out int userId))
                {
                    return APIResponse<TokenResponse>.Failure(["Invalid user or token"]);
                }

                var user = await _userRepository.GetUserAsync(userId);
                if (user == null)
                    return APIResponse<TokenResponse>.Failure(["Invalid user or token"]);

                if (user.IsLocked)
                {
                    return APIResponse<TokenResponse>.Failure(["The account has been locked"]);
                }
                if (!user.IsVerified)
                {
                    return APIResponse<TokenResponse>.Failure(["The account is inactive, please verify the account"]);
                }

                var accessToken = _jwtTokenService.GenerateAccessToken(user, true);

                var response = new TokenResponse()
                {
                    AccessToken = accessToken,
                    RefreshToken = _jwtTokenService.GenerateRefreshToken()
                };

                var securityToken = JwtTokenHelper.GetJwtSecurityToken(accessToken);

                await _userTokenRepository.DeleteToken(Guid.Parse(securityToken.Id));
                await _userTokenRepository.InsertToken(new Core.Entities.Systems.UserToken
                {
                    RefreshToken = response.RefreshToken,
                    TokenID = Guid.Parse(securityToken.Id),
                    UserID = user.ItemID,
                    ValidTo = DateTime.UtcNow.AddDays(7)
                });

                return APIResponse<TokenResponse>.Success(response, ["Authenticated"]);
            }
            return APIResponse<TokenResponse>.Failure(["Invalid user or token"]);
        }
    }
}
