using LumStoreAPI.Core.Models.Enums;

namespace LumStoreAPI.Application.DTOs.UserDTO;

public class UserAdminRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Search { get; set; }
    public UserRole? UserRole { get; set; }
}