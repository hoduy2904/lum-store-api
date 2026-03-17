using LumStoreAPI.Application.DTOs.AuthDTO;
using LumStoreAPI.Application.DTOs.Responses;
using LumStoreAPI.Application.DTOs.UserDTO;
using LumStoreAPI.Application.Interfaces;
using LumStoreAPI.Core.Interfaces.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LumStoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IUserRepository _userRepository;
        private readonly IUserService _userService;
        public AuthController(IAuthService authService, IUserRepository userRepository, IUserService userService)
        {
            _authService = authService;
            _userRepository = userRepository;
            _userService = userService;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register(UserCreateRequest userCreateRequest)
        {
            var user = await _authService.RegisterUserAsync(userCreateRequest);
            return Ok(user);
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
    }
}
