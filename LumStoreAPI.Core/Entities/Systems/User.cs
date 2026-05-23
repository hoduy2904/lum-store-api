using LumStoreAPI.Core.Entities.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace LumStoreAPI.Core.Entities.Systems
{
    public class User : BaseClassItem
    {
        public string UserName { get; set; } = default!;
        public string UserPassword { get; set; } = default!;
        public string FirstName { get; set; } = default!;
        public string? MiddleName { get; set; }
        public string LastName { get; set; } = default!;

        [NotMapped]
        public string FullName
        {
            get
            {
                return $"{this.FirstName.Trim()} {this.MiddleName?.Trim() ?? ""} {this.LastName.Trim()}";
            }
        }
        public string Email { get; set; } = default!;
        public string? Avatar { get; set; }
        public bool IsLocked { get; set; }
        public bool IsVerified { get; set; }
        public bool IsEnabled { get; set; }
        public DateTimeOffset TimeLocked { get; set; }
        public int UserLevel { get; set; }
        public string? VerifyCode { get; set; }
        public bool IsAdmin { get; set; }
        public DateTimeOffset? TimeActionCode { get; set; }

        public string? GoogleId { get; set; }
        public string? FacebookId { get; set; }
        public string? Provider { get; set; }

        public virtual ICollection<UserToken> UserTokens { get; set; } = [];
        public virtual ICollection<EventLog> EventLogs { get; set; } = [];
    }
}
