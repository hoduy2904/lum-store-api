using LumStoreAPI.Core.Entities.Systems;
using LumStoreAPI.Core.Interfaces.ContentEngine;
using LumStoreAPI.Core.Models.Enums;
using LumStoreAPI.Core.Models.Systems;
using LumStoreAPI.Infrastructure.Models;

namespace LumStoreAPI.Core.Interfaces.Sytems
{
    public interface IEmailService
    {
        Task SendEmailAsync(EmailMessage emailMessage);
        Task<EmailConfig> GetConfigAsync();
        Task<IEnumerable<EmailQueue>> GetEmailQueuesAsync(int topN);
        Task<IPagedEnumerable<EmailQueue>> GetEmailQueuesAsync(int page, int pageSize, EmailStatus? emailStatus = null, string? q = null);
        Task<int> UpdateEmailStatusAsync(int emailID, EmailStatus emailStatus);
    }
}
