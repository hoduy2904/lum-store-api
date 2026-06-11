using LumStoreAPI.Application.DTOs.AuthDTO;
using LumStoreAPI.Application.DTOs.Responses;
using LumStoreAPI.Application.DTOs.UserDTO;
using LumStoreAPI.Application.Interfaces;
using LumStoreAPI.Core.Models.Constants.Systems;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LumStoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IUserService _userService;
        private readonly ISocialAuthService _socialAuthService;
        public AuthController(IAuthService authService, IUserService userService, ISocialAuthService socialAuthService)
        {
            _authService = authService;
            _userService = userService;
            _socialAuthService = socialAuthService;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register(UserCreateRequest userCreateRequest)
        {
            var result = await _authService.RegisterUserAsync(userCreateRequest);
            return Ok(result);
        }

        [HttpPost("Verify")]
        [Authorize(Roles = "pre")]
        public async Task<IActionResult> VerifyCode(string code)
        {
            return Ok(await _authService.VerifyCode(code));
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(AuthRequest auth)
        {
            var token = await _authService.AuthenticateAsync(auth);
            return Ok(token);
        }

        [HttpPost("Logout")]
        public async Task<IActionResult> Logout()
        {
            await _authService.LogoutAsync();
            return NoContent();
        }

        [HttpGet("CurrentUser")]
        [Authorize]
        public async Task<IActionResult> CurrentUser()
        {
            var user = await _userService.GetCurrentUserAsync();
            return Ok(APIResponse<UserDTO?>.Success(user));
        }

        [HttpPost("ChangePassword")]
        [Authorize]
        public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
        {
            return Ok(await _authService.ChangePasswordAsync(request));
        }

        [HttpPost("ForgotPassword")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request)
        {
            return Ok(await _authService.ForgotPasswordAsync(request));
        }

        [HttpPost("ResetPassword")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
        {
            return Ok(await _authService.ResetPasswordAsync(request));
        }

        [HttpPost("social-login")]
        [AllowAnonymous]
        public async Task<IActionResult> SocialLogin(SocialLoginRequest request)
        {
            return Ok(await _socialAuthService.SocialLoginAsync(request));
        }

        [HttpPost("RefreshToken")]
        public async Task<IActionResult> RefreshToken()
        {
            string accessToken = Request.Cookies[AuthSystemConstants.ACCESS_TOKEN_COOKIE_NAME]!;
            string? refreshToken = Request.Cookies[AuthSystemConstants.REFRESH_TOKEN_COOKIE_NAME];
            if (string.IsNullOrEmpty(refreshToken))
            {
                return Unauthorized();
            }
            var token = await _authService.RefreshTokenAsync(new(accessToken, refreshToken));
            return Ok(token);
        }
    }
}
