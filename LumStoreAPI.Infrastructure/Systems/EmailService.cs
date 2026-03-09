using LumStoreAPI.Core.Entities.Systems;
using LumStoreAPI.Core.Interfaces.ContentEngine;
using LumStoreAPI.Core.Interfaces.Repositories;
using LumStoreAPI.Core.Interfaces.Sytems;
using LumStoreAPI.Core.Models.Constants;
using LumStoreAPI.Core.Models.Enums;
using LumStoreAPI.Core.Models.Systems;
using LumStoreAPI.Infrastructure.Models;

namespace LumStoreAPI.Infrastructure.Systems
{
    internal class EmailService : IEmailService
    {
        private readonly ISettingKeyValueRepository _settingKeyValueRepository;
        private readonly IEmailRepository _emailRepository;
        public EmailService(ISettingKeyValueRepository settingKeyValueRepository, IEmailRepository emailRepository)
        {
            _settingKeyValueRepository = settingKeyValueRepository;
            _emailRepository = emailRepository;
        }

        public async Task<EmailConfig> GetConfigAsync()
        {
            string[] emailConfigKeys = [SystemKeyConstants.EMAIL_PASSWORD, SystemKeyConstants.EMAIL_PORT,
                    SystemKeyConstants.EMAIL_HOST, SystemKeyConstants.EMAIL_USERNAME];

            var emailConfigData = (await _settingKeyValueRepository
                  .GetSettingKeysAsync(x => emailConfigKeys.Contains(x.SettingCode)))
                  .ToDictionary(x => x.SettingCode, x => x.SettingValue);

            var emailConfig = new EmailConfig(
                emailConfigData.GetValueOrDefault(SystemKeyConstants.EMAIL_HOST, ""),
                int.Parse(emailConfigData.GetValueOrDefault(SystemKeyConstants.EMAIL_PORT, "0")),
                emailConfigData.GetValueOrDefault(SystemKeyConstants.EMAIL_USERNAME, ""),
                emailConfigData.GetValueOrDefault(SystemKeyConstants.EMAIL_PASSWORD, "")
             );

            return emailConfig;
        }

        public async Task<IEnumerable<EmailQueue>> GetEmailQueuesAsync(int topN)
        {
            return await _emailRepository.GetEmailQueuesAsync(
                x => x.OrderBy(x => x.ItemID)
                .ThenBy(x => x.EmailStatus)
                .Where(x => x.EmailStatus != Core.Models.Enums.EmailStatus.Success)
                .Take(topN));
        }

        public async Task<IPagedEnumerable<EmailQueue>> GetEmailQueuesAsync(int page, int pageSize, EmailStatus? emailStatus = null, string? q = null)
        {
            return await _emailRepository.GetEmailQueuesAsync(page, pageSize,
                x => (string.IsNullOrEmpty(q) || x.EmailFrom.Contains(q)) &&
                emailStatus == null || x.EmailStatus == emailStatus);
        }

        public Task SendEmailAsync(EmailMessage emailMessage)
        {
            return _emailRepository.InsertEmailQueueAsync(emailMessage.GetEmailQueue());
        }

        public Task<int> UpdateEmailStatusAsync(int emailID, EmailStatus emailStatus)
        {
            return _emailRepository.UpdateEmailAsync(emailID, emailStatus);
        }
    }
}
