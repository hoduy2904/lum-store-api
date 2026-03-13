using LumStoreAPI.Application.DTOs.AuthDTO;
using LumStoreAPI.Application.DTOs.UserDTO;
using LumStoreAPI.Application.Interfaces;
using LumStoreAPI.Core.Interfaces.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace LumStoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IUserRepository _userRepository;
        public AuthController(IAuthService authService, IUserRepository userRepository)
        {
            _authService = authService;
            _userRepository = userRepository;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register(UserCreateRequest userCreateRequest)
        {
            var user = await _userRepository.InsertUserAsync(userCreateRequest.GetEntity);
            return Ok(user);
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(AuthRequest auth)
        {
            var token = await _authService.AuthenticateAsync(auth);
            return Ok(token);
        }
    }
}
