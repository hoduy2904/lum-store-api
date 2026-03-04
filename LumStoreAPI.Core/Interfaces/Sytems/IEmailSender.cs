using LumStoreAPI.Core.Models.Systems;

namespace LumStoreAPI.Core.Interfaces.Sytems
{
    public interface IEmailSender
    {
        Task SendEmail(EmailMessage emailMessage);
        Task SendEmailAsync(EmailMessage emailMessage);
    }
}
