using LumStoreAPI.Core.Entities.Systems;

namespace LumStoreAPI.Application.DTOs.UserDTO;

public class UserDTO
{
    public string UserName { get; set; } = default!;
    public string FirstName { get; set; } = default!;
    public string? MiddleName { get; set; }
    public string LastName { get; set; } = default!;
    public string FullName
    {
        get
        {
            return $"{this.FirstName.Trim()} {this.MiddleName?.Trim() ?? ""} {this.LastName.Trim()}";
        }
    }
    public string Email { get; set; } = default!;
    public string? Avatar { get; set; }
    public string UserRole { get; set; } = "user";

    public UserDTO(User user)
    {
        this.LastName = user.LastName;
        this.FirstName = user.FirstName;
        this.MiddleName = user.MiddleName;
        this.Email = user.Email;
        this.Avatar = user.Avatar;
        this.UserRole = user.IsAdmin ? "admin" : "user";
    }
}
