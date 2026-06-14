using LumStoreAPI.Core.Models.Systems;
using LumStoreAPI.Core.Models.Systems.SettingKeys;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Scriban;

namespace LumStoreAPI.Libraries.Helpers
{
    public class EmailHelper
    {
        private static SmtpClient? _client = null;
        public static async Task SendMail(EmailMessage emailMessage, EmailSettings emailConfig, CancellationToken cancellationToken = default)
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
                await SendEmail(mailMessage, emailConfig, cancellationToken);
            }
        }

        private static async Task SendEmail(MimeMessage mimeMessage, EmailSettings emailConfig, CancellationToken cancellationToken = default)
        {
            if (_client == null)
            {
                _client = new SmtpClient();
            }
            if (!_client.IsConnected)
            {
                await _client.ConnectAsync(emailConfig.Host, emailConfig.Port, SecureSocketOptions.StartTls, cancellationToken);
            }

            if (!_client.IsAuthenticated)
            {
                await _client.AuthenticateAsync(emailConfig.Username, emailConfig.Password, cancellationToken);
            }
            try
            {
                await _client.SendAsync(mimeMessage, cancellationToken);
            }
            catch
            {
                await DisconnectAsync(cancellationToken);
                throw;
            }
        }

        public static async Task DisconnectAsync(CancellationToken cancellationToken = default)
        {
            if (_client == null)
            {
                return;
            }

            try
            {
                if (_client.IsConnected)
                {
                    await _client.DisconnectAsync(true, cancellationToken);
                }
            }
            finally
            {
                _client.Dispose();
                _client = null;
            }
        }

        public static async Task<EmailTemplate> MacroEmailTemplate(EmailTemplate emailTemplate, object data)
        {
            var headerTemplate = Template.Parse(emailTemplate.EmailHeader);
            var bodyTemplate = Template.Parse(emailTemplate.EmailBody);

            var emailHeaderText = await headerTemplate.RenderAsync(data, memberRenamer: member => member.Name);
            var emailBodyText = await bodyTemplate.RenderAsync(data, memberRenamer: member => member.Name);

            return new EmailTemplate
            {
                EmailBody = emailBodyText,
                EmailHeader = emailHeaderText
            };
        }
    }
}
