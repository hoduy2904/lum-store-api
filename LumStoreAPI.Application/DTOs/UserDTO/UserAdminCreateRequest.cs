using System.Text.Json.Serialization;
using LumStoreAPI.Core.Entities.Systems;
using LumStoreAPI.Core.Models.Enums;

namespace LumStoreAPI.Application.DTOs.UserDTO;

public class UserAdminCreateRequest : UserCreateRequest
{
    public UserRole UserRole { get; set; } = UserRole.USER;
    public string? Avatar { get; set; }
    [JsonIgnore]
    public override User GetEntity
    {
        get
        {
            var entity = base.GetEntity;
            entity.Role = UserRole;
            entity.Avatar = Avatar;
            return entity;
        }
    }
}