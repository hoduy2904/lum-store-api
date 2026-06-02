using LumStoreAPI.Core.Models.Systems;
using LumStoreAPI.Core.Models.Systems.SettingKeys;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace LumStoreAPI.Libraries.Helpers
{
    public class EmailHelper
    {
        public static async Task SendMail(EmailMessage emailMessage, EmailSettings emailConfig)
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
                await SendEmail(mailMessage, emailConfig);
            }
        }

        private static async Task SendEmail(MimeMessage mimeMessage, EmailSettings emailConfig)
        {
            using var client = new SmtpClient();
            await client.ConnectAsync(emailConfig.Host, emailConfig.Port, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(emailConfig.Username, emailConfig.Password);
            await client.SendAsync(mimeMessage);
            await client.DisconnectAsync(true);
        }
    }
}
