using LumStoreAPI.Core.Entities.Base;
using LumStoreAPI.Core.Models.Enums;

namespace LumStoreAPI.Core.Entities.Systems
{
    public class EmailQueue : BaseClassItem
    {
        public EmailStatus EmailStatus { get; set; } = EmailStatus.Waiting;
        public string EmailFrom { get; set; } = default!;
        public string[] EmailTo { get; set; } = default!;
        public string EmailSubject { get; set; } = default!;
        public string EmailBody { get; set; } = default!;
        public string[]? EmailBcc { get; set; }
        public string[]? EmailCc { get; set; }
        public string[]? Attachments { get; set; }
        public DateTimeOffset? NextRetryTime { get; set; }
    }
}
