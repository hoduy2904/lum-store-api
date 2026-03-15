using LumStoreAPI.Core.Entities.Systems;
using LumStoreAPI.Libraries.Helpers;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace LumStoreAPI.Application.DTOs.UserDTO
{
    public class UserCreateRequest
    {
        public string FirstName { get; set; } = default!;
        public string? MiddleName { get; set; }
        public string LastName { get; set; } = default!;
        public string Email { get; set; } = default!;
        [MaxLength(50)]
        public string UserName { get; set; } = default!;
        public string Password { get; set; } = default!;
        [JsonIgnore]
        public string PasswordHash => HashHelper.HashPassword(Password);

        [JsonIgnore]
        public User GetEntity => new User()
        {
            UserName = UserName,
            Email = Email,
            FirstName = FirstName,
            LastName = LastName,
            MiddleName = MiddleName,
            UserPassword = PasswordHash,
        };

    }
}
