using LumStoreAPI.Core.Entities.Systems;
using LumStoreAPI.Core.Models.Enums;

namespace LumStoreAPI.Application.DTOs.UserDTO;

public class UserDTO
{
    public int UserId { get; set; }
    public string UserName { get; set; } = default!;
    public string FirstName { get; set; } = default!;
    public string? MiddleName { get; set; }
    public string LastName { get; set; } = default!;
    public string FullName => $"{this.FirstName.Trim()} {this.MiddleName?.Trim() ?? ""} {this.LastName.Trim()}";
    public string Email { get; set; } = default!;
    public string? Avatar { get; set; }
    public UserRole UserRole { get; set; }
    public bool IsLocked { get; set; }
    public bool IsVerified { get; set; }
    public bool IsEnabled { get; set; }

    public UserDTO(User user)
    {
        this.UserId = user.ItemID;
        this.LastName = user.LastName;
        this.FirstName = user.FirstName;
        this.MiddleName = user.MiddleName;
        this.Email = user.Email;
        this.Avatar = user.Avatar;
        this.UserRole = user.Role;
        this.UserName = user.UserName;
        this.IsLocked = user.IsLocked;
        this.IsVerified = user.IsVerified;
        this.IsEnabled = user.IsEnabled;
    }
}
