using LumStoreAPI.Core.Models.Enums;

namespace LumStoreAPI.Application.DTOs.UserDTO;

public class UserAdminUpdateRequest
{
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string? MiddleName { get; set; }
    public string Email { get; set; } = default!;
    public string? Avatar { get; set; }
    public UserRole UserRole { get; set; }
    public string? Password { get; set; }
    public bool IsLocked { get; set; }
    public bool IsVerified { get; set; }
    public bool IsEnabled { get; set; }
}