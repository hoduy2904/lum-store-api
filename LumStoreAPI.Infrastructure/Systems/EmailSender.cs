using LumStoreAPI.Core.Interfaces.Sytems;
using LumStoreAPI.Core.Models.Systems;

namespace LumStoreAPI.Infrastructure.Systems
{
    internal class EmailSender : IEmailSender
    {
        public Task SendEmail(EmailMessage emailMessage)
        {
            throw new NotImplementedException();
        }

        public Task SendEmailAsync(EmailMessage emailMessage)
        {
            throw new NotImplementedException();
        }
    }
}
