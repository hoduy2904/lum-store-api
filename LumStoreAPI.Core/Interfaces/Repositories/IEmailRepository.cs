using LumStoreAPI.Core.Entities.Systems;
using LumStoreAPI.Core.Interfaces.ContentEngine;
using LumStoreAPI.Core.Models.Enums;
using System.Linq.Expressions;

namespace LumStoreAPI.Core.Interfaces.Repositories
{
    public interface IEmailRepository
    {
        Task<IEnumerable<EmailQueue>> GetEmailQueuesAsync(Func<IQueryable<EmailQueue>, IQueryable<EmailQueue>>? action = null);
        Task<IPagedEnumerable<EmailQueue>> GetEmailQueuesAsync(int page, int pageSize, Expression<Func<EmailQueue, bool>>? expression = null);
        Task<EmailQueue> InsertEmailQueueAsync(EmailQueue emailQueue);
        Task<int> DeleteEmailQueueAsync(int id);
        Task<int> DeleteEmailQueuesAsync(Expression<Func<EmailQueue, bool>> expression);
        Task<int> UpdateEmailAsync(int emailID, EmailStatus emailStatus);
    }
}
