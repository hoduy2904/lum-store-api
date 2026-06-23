using LumStoreAPI.Application.DTOs.Responses;
using LumStoreAPI.Application.DTOs.UserDTO;
using LumStoreAPI.Application.Interfaces;
using LumStoreAPI.Core.Interfaces.ContentEngine;
using LumStoreAPI.Core.Models.Constants.Systems;
using LumStoreAPI.Core.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LumStoreAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Policy = nameof(RoleType.ADMIN_TYPE))]
public class UserController(
    IUserService userService) : ControllerBase
{
    private readonly IUserService _userService = userService;
    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] UserAdminRequest request)
    {
        var users = await _userService.GetUsersAsync(request);
        return Ok(PagedResponse<UserDTO>.Success(users, request.Page, request.PageSize));
    }

    [HttpGet("{userId:int}")]
    public async Task<IActionResult> GetUser(int userId)
    {
        var user = await _userService.GetUserAsync(userId);
        if (user == null)
        {
            return NotFound(APIResponseBase.Failure(ErrorStatusNameConstants.NOT_FOUND, ["Not found user with id " + userId]));
        }
        return Ok(APIResponse<UserDTO>.Success(user));
    }

    [HttpPost]
    public async Task<IActionResult> InsertUser(UserAdminCreateRequest request)
    {
        var user = await _userService.CreateUserAsync(request);
        if(user == null) return BadRequest(APIResponseBase.Failure(ErrorStatusNameConstants.INVALID_DATA));
        return Ok(APIResponse<UserDTO>.Success(user));
    }

    [HttpPut("{userId:int}")]
    public async Task<IActionResult> UpdateUser(int userId, UserAdminUpdateRequest request)
    {
        var updateUser = await _userService.UpdateUserAsync(userId, request);
        if (updateUser == null) return BadRequest(APIResponseBase.Failure(ErrorStatusNameConstants.INVALID_DATA,["Please try later"]));
        return Ok(APIResponse<UserDTO>.Success(updateUser));
    }
}