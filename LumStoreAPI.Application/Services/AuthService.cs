using LumStoreAPI.Application.DTOs.AuthDTO;
using LumStoreAPI.Application.DTOs.Responses;
using LumStoreAPI.Application.DTOs.UserDTO;
using LumStoreAPI.Application.Exceptions;
using LumStoreAPI.Application.Interfaces;
using LumStoreAPI.Core.Interfaces.Repositories;
using LumStoreAPI.Core.Interfaces.Services;
using LumStoreAPI.Core.Interfaces.Sytems;
using LumStoreAPI.Core.Models.Constants.Systems;
using LumStoreAPI.Core.Models.Systems;
using LumStoreAPI.Libraries.Helpers;
using Microsoft.AspNetCore.Http;

namespace LumStoreAPI.Application.Services
{
    internal class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IUserTokenRepository _userTokenRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IEmailService _emailService;
        public AuthService(
            IUserRepository userRepository,
            IJwtTokenService jwtTokenService,
            IUserTokenRepository userTokenRepository,
            IHttpContextAccessor httpContextAccessor,
            IEmailService emailService)
        {
            _userRepository = userRepository;
            _jwtTokenService = jwtTokenService;
            _userTokenRepository = userTokenRepository;
            _httpContextAccessor = httpContextAccessor;
            _emailService = emailService;
        }

        DateTime REFRESH_TOKEN_TIME => DateTime.UtcNow.AddDays(7);
        CookieOptions cookieOptions => new CookieOptions()
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Path = "/"
        };

        public async Task<APIResponse<TokenResponse>> AuthenticateAsync(AuthRequest request)
        {
            var user = (await _userRepository.GetUsersAsync(query =>
            {
                query = query.Where(x => x.UserName.Equals(request.UserName) && x.IsEnabled).Take(1);
                return query;
            })).FirstOrDefault();

            if (user == null || !HashHelper.VerifyPassword(request.Password, user.UserPassword))
            {
                return APIResponse<TokenResponse>.Failure(ErrorStatusNameConstants.NOT_FOUND, ["Invalid username or password"]);
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

            SetCookies(accessToken, response.RefreshToken);

            return APIResponse<TokenResponse>.Success(response, ["Authenticated"]);

        }

        public Task<bool> IsValidCodeAsync(int userID, string code)
        {
            return _userRepository.CheckUserAsync(x => x.ItemID == userID && !string.IsNullOrEmpty(x.VerifyCode) && x.VerifyCode.Equals(code.Trim()));
        }

        public Task LogoutAsync()
        {
            var logoutCookieOpt = cookieOptions;

            _httpContextAccessor.HttpContext?.Response.Cookies.Delete(AuthSystemConstants.ACCESS_TOKEN_COOKIE_NAME);
            _httpContextAccessor.HttpContext?.Response.Cookies.Delete(AuthSystemConstants.REFRESH_TOKEN_COOKIE_NAME);

            return Task.CompletedTask;
        }

        public async Task<APIResponse<TokenResponse>> RefreshTokenAsync(TokenRequest request)
        {
            var tokenSecurity = JwtTokenHelper.GetJwtSecurityToken(request.accessToken);
            if (await _userTokenRepository.IsValidRefreshToken(Guid.Parse(tokenSecurity.Id), request.refreshToken))
            {
                if (!int.TryParse(tokenSecurity.Claims.FirstOrDefault(x => x.Type.Equals("id"))?.Value, out int userId))
                {
                    return APIResponse<TokenResponse>.Failure(ErrorStatusNameConstants.NOT_FOUND, ["Invalid user or token"]);
                }

                var user = await _userRepository.GetUserAsync(userId);
                if (user == null)
                    return APIResponse<TokenResponse>.Failure(ErrorStatusNameConstants.NOT_FOUND, ["Invalid user or token"]);

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
                    ValidTo = REFRESH_TOKEN_TIME
                });

                SetCookies(accessToken, response.RefreshToken);
                return APIResponse<TokenResponse>.Success(response, ["Authenticated"]);
            }
            return APIResponse<TokenResponse>.Failure(ErrorStatusNameConstants.NOT_FOUND, ["Invalid user or token"]);
        }

        public async Task<UserDTO> RegisterUserAsync(UserCreateRequest request)
        {
            var entity = request.GetEntity;
            entity.VerifyCode = StringHelper.GenerateCode();
            entity.TimeActionCode = DateTime.UtcNow;
            var user = await _userRepository.InsertUserAsync(entity);

            await _emailService.SendEmailAsync(new EmailMessage
            {
                EmailTo = [request.Email],
                EmailSubject = "Register Account",
                EmailFrom = "noreply@lumstore.com",
                EmailBody = "Code: " + entity.VerifyCode
            });

            return new UserDTO(user);
        }

        public Task<bool> ResendVerifyCodeAsync(int user)
        {
            throw new NotImplementedException();
        }

        public async Task<APIResponseBase> VerifyCode(string code)
        {
            if (!int.TryParse(_httpContextAccessor.HttpContext?.User.Claims.FirstOrDefault(x => x.Type.Equals("id"))?.Value, out int userId))
            {
                throw new ForbidException("Invalid user");
            }
            var user = await _userRepository.UpdateUsersAsync(
                x => x.ItemID == userId && x.VerifyCode != null && x.VerifyCode.Equals(code.Trim()),
                x => x.Set(p => p.VerifyCode, (string?)null)
                .Set(p => p.IsEnabled, true)
                .Set(p => p.IsLocked, false)
                .Set(p => p.IsVerified, true));

            return user > 0 ? APIResponseBase.Success(["Verified, please login"]) : APIResponseBase.Failure(ErrorStatusNameConstants.NOT_FOUND, ["Invalid verify code, please try again"]);
        }

        private void SetCookies(string accessToken, string refreshToken)
        {
            var securityToken = JwtTokenHelper.GetJwtSecurityToken(accessToken);

            var accessCookieOption = cookieOptions;
            accessCookieOption.Expires = securityToken.ValidTo;
            _httpContextAccessor?.HttpContext?.Response.Cookies.Append(AuthSystemConstants.ACCESS_TOKEN_COOKIE_NAME, accessToken, accessCookieOption);


            var refreshCookieOption = cookieOptions;
            accessCookieOption.Expires = REFRESH_TOKEN_TIME;
            _httpContextAccessor?.HttpContext?.Response.Cookies.Append(AuthSystemConstants.REFRESH_TOKEN_COOKIE_NAME, refreshToken, refreshCookieOption);
        }
    }
}
