using LumStoreAPI.Core.Entities.Systems;
using LumStoreAPI.Core.Interfaces.ContentEngine;
using LumStoreAPI.Core.Interfaces.Repositories;
using LumStoreAPI.Core.Interfaces.Services;
using LumStoreAPI.Core.Interfaces.Sytems;
using LumStoreAPI.Core.Models.Enums;
using LumStoreAPI.Core.Models.Systems;
using LumStoreAPI.Core.Models.Systems.SettingKeys;

namespace LumStoreAPI.Infrastructure.Systems
{
    internal class EmailService : IEmailService
    {
        private readonly ISettingKeyValueService _settingKeyValueService;
        private readonly IEmailRepository _emailRepository;
        public EmailService(ISettingKeyValueService settingKeyValueService, IEmailRepository emailRepository)
        {
            _settingKeyValueService = settingKeyValueService;
            _emailRepository = emailRepository;
        }

        public async Task<EmailSettings> GetConfigAsync()
        {
            return (await _settingKeyValueService.GetSystemSettingAsync<EmailSettings>()) ?? new EmailSettings();

        }

        public async Task<IEnumerable<EmailQueue>> GetEmailQueuesAsync(int topN)
        {
            return await _emailRepository.GetEmailQueuesAsync(
                x => x.OrderBy(x => x.ItemID)
                .ThenBy(x => x.EmailStatus)
                .Where(x => x.EmailStatus != Core.Models.Enums.EmailStatus.Success
                && (x.NextRetryTime == null || x.NextRetryTime < DateTime.UtcNow))
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
