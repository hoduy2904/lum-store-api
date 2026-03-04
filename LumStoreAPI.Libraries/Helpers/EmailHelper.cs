using LumStoreAPI.Core.Models.Systems;
using LumStoreAPI.Infrastructure.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace LumStoreAPI.Libraries.Helpers
{
    public class EmailHelper
    {
        public static Task SendMail(EmailMessage emailMessage, EmailConfig emailConfig)
        {
            using (var mailMessage = new MimeMessage())
            {
                mailMessage.From.Add(new MailboxAddress(emailMessage.EmailFrom, emailMessage.EmailFrom));
                mailMessage.To.AddRange(
                    emailMessage.EmailTo.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => new MailboxAddress(x, x)));

                if (emailMessage.EmailCc != null)
                {
                    mailMessage.Bcc.AddRange(
                        emailMessage.EmailCc.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => new MailboxAddress(x, x)));
                }
                if (emailMessage.EmailBcc != null)
                {
                    mailMessage.To.AddRange(
                        emailMessage.EmailBcc.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => new MailboxAddress(x, x)));
                }

                mailMessage.Subject = emailMessage.EmailSubject;

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = emailMessage.EmailBody,
                };


                if (emailMessage.Attachments != null)
                {
                    foreach (var attachment in emailMessage.Attachments)
                    {
                        bodyBuilder.Attachments.Add(attachment.FileName, attachment.Stream);
                    }
                }
                mailMessage.Body = bodyBuilder.ToMessageBody();
                return SendEmail(mailMessage, emailConfig);
            }
        }

        private static async Task SendEmail(MimeMessage mimeMessage, EmailConfig emailConfig)
        {
            using var client = new SmtpClient();
            await client.ConnectAsync(emailConfig.host, emailConfig.port, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(emailConfig.username, emailConfig.password);
            await client.SendAsync(mimeMessage);
            await client.DisconnectAsync(true);
        }
    }
}
