using LumStoreAPI.Core.Entities.Systems;

namespace LumStoreAPI.Core.Models.Systems
{
    public class EmailMessage
    {
        public string EmailFrom { get; set; } = default!;
        public string[] EmailTo { get; set; } = default!;
        public string EmailSubject { get; set; } = default!;
        public string EmailBody { get; set; } = default!;
        public string[]? EmailBcc { get; set; }
        public string[]? EmailCc { get; set; }
        public EmailAttachment[]? Attachments { get; set; }

        public EmailQueue GetEmailQueue()
        {
            return new EmailQueue
            {
                Attachments = Attachments?.Select(x => x.FileID.ToString()).ToArray(),
                EmailSubject = EmailSubject,
                EmailBcc = EmailBcc,
                EmailBody = EmailBody,
                EmailCc = EmailCc,
                EmailFrom = EmailFrom,
                EmailStatus = Enums.EmailStatus.Waiting,
                EmailTo = EmailTo,
            };
        }
    }
}
