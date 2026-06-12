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
using LumStoreAPI.Core.Models.Systems.SettingKeys;
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
        private readonly ISettingKeyValueService _settingKeyValueService;
        public AuthService(
            IUserRepository userRepository,
            IJwtTokenService jwtTokenService,
            IUserTokenRepository userTokenRepository,
            IHttpContextAccessor httpContextAccessor,
            IEmailService emailService,
            ISettingKeyValueService settingKeyValueService)
        {
            _userRepository = userRepository;
            _jwtTokenService = jwtTokenService;
            _userTokenRepository = userTokenRepository;
            _httpContextAccessor = httpContextAccessor;
            _emailService = emailService;
            _settingKeyValueService = settingKeyValueService;
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

        public async Task LogoutAsync()
        {
            string? token = _httpContextAccessor.HttpContext?.Request.Cookies[AuthSystemConstants.ACCESS_TOKEN_COOKIE_NAME];
            if (string.IsNullOrWhiteSpace(token)) return;

            var tokenInfo = JwtTokenHelper.GetJwtSecurityToken(token);

            if (Guid.TryParse(tokenInfo.Id, out Guid jwtId))
            {
                await _userTokenRepository.DeleteToken(jwtId);
            }

            _httpContextAccessor.HttpContext?.Response.Cookies.Delete(AuthSystemConstants.ACCESS_TOKEN_COOKIE_NAME);
            _httpContextAccessor.HttpContext?.Response.Cookies.Delete(AuthSystemConstants.REFRESH_TOKEN_COOKIE_NAME);

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

        public async Task<APIResponse<TokenResponse>> RegisterUserAsync(UserCreateRequest request)
        {
            var entity = request.GetEntity;
            entity.VerifyCode = StringHelper.GenerateCode();
            entity.TimeActionCode = DateTime.UtcNow;
            var user = await _userRepository.InsertUserAsync(entity);
            var setting = await _settingKeyValueService.GetSystemSettingAsync<EmailSettings>();

            await _emailService.SendEmailAsync(new EmailMessage
            {
                EmailTo = [request.Email],
                EmailSubject = "Register Account",
                EmailFrom = setting?.FromEmail ?? "noreply@lumnails.com",
                EmailBody = "Code: " + entity.VerifyCode
            });

            var accessToken = _jwtTokenService.GenerateAccessToken(user, false);
            var refreshToken = _jwtTokenService.GenerateRefreshToken();
            var securityToken = JwtTokenHelper.GetJwtSecurityToken(accessToken);

            await _userTokenRepository.InsertToken(new Core.Entities.Systems.UserToken
            {
                RefreshToken = refreshToken,
                TokenID = Guid.Parse(securityToken.Id),
                UserID = user.ItemID,
                ValidTo = DateTime.UtcNow.AddDays(7)
            });

            SetCookies(accessToken, refreshToken);

            return APIResponse<TokenResponse>.Success(new TokenResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            }, ["Registered successfully, please verify your email"]);
        }

        public Task<bool> ResendVerifyCodeAsync(int user)
        {
            throw new NotImplementedException();
        }

        public async Task<APIResponseBase> ForgotPasswordAsync(ForgotPasswordRequest request)
        {
            var user = (await _userRepository.GetUsersAsync(query =>
                query.Where(x => x.Email.Equals(request.Email) && x.IsEnabled && x.IsVerified).Take(1)
            )).FirstOrDefault();

            // Always return success to avoid email enumeration
            if (user == null)
                return APIResponseBase.Success(["If your email is registered, you will receive a reset code."]);

            var code = StringHelper.GenerateCode();
            var setting = await _settingKeyValueService.GetSystemSettingAsync<EmailSettings>();

            await _userRepository.UpdateUsersAsync(
                x => x.ItemID == user.ItemID,
                x => x.Set(p => p.VerifyCode, code)
                       .Set(p => p.TimeActionCode, _ => (DateTimeOffset?)DateTimeOffset.UtcNow));

            await _emailService.SendEmailAsync(new EmailMessage
            {
                EmailTo = [user.Email],
                EmailSubject = "Reset Password",
                EmailFrom = setting?.FromEmail ?? "noreply@lumstore.com",
                EmailBody = "Your password reset code is: " + code + ". This code is valid for 15 minutes."
            });

            return APIResponseBase.Success(["If your email is registered, you will receive a reset code."]);
        }

        public async Task<APIResponseBase> ResetPasswordAsync(ResetPasswordRequest request)
        {
            var expiry = DateTimeOffset.UtcNow.AddMinutes(-15);

            var updated = await _userRepository.UpdateUsersAsync(
                x => x.Email.Equals(request.Email)
                     && x.IsEnabled
                     && x.VerifyCode != null
                     && x.VerifyCode.Equals(request.Code.Trim())
                     && x.TimeActionCode != null
                     && x.TimeActionCode >= expiry,
                x => x.Set(p => p.UserPassword, HashHelper.HashPassword(request.NewPassword))
                       .Set(p => p.VerifyCode, (string?)null)
                       .Set(p => p.TimeActionCode, (DateTimeOffset?)null));

            return updated > 0
                ? APIResponseBase.Success(["Password reset successfully. You can now log in."])
                : APIResponseBase.Failure(ErrorStatusNameConstants.NOT_FOUND, ["Invalid or expired reset code."]);
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

        public async Task<APIResponseBase> ChangePasswordAsync(ChangePasswordRequest request)
        {
            if (!int.TryParse(_httpContextAccessor.HttpContext?.User.Claims.FirstOrDefault(x => x.Type.Equals("id"))?.Value, out int userId))
                throw new ForbidException("Invalid user");

            var user = await _userRepository.GetUserAsync(userId);
            if (user == null || !HashHelper.VerifyPassword(request.CurrentPassword, user.UserPassword))
                return APIResponseBase.Failure(ErrorStatusNameConstants.NOT_FOUND, ["Current password is incorrect."]);

            await _userRepository.UpdateUsersAsync(
                x => x.ItemID == userId,
                x => x.Set(p => p.UserPassword, HashHelper.HashPassword(request.NewPassword)));

            return APIResponseBase.Success(["Password changed successfully."]);
        }

        private void SetCookies(string accessToken, string refreshToken)
        {
            var accessCookieOption = cookieOptions;
            accessCookieOption.Expires = REFRESH_TOKEN_TIME;
            _httpContextAccessor?.HttpContext?.Response.Cookies.Append(AuthSystemConstants.ACCESS_TOKEN_COOKIE_NAME, accessToken, accessCookieOption);


            var refreshCookieOption = cookieOptions;
            refreshCookieOption.Expires = REFRESH_TOKEN_TIME;
            _httpContextAccessor?.HttpContext?.Response.Cookies.Append(AuthSystemConstants.REFRESH_TOKEN_COOKIE_NAME, refreshToken, refreshCookieOption);
        }
    }
}
