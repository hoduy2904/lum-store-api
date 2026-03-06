using LumStoreAPI.Core.Entities.Systems;
using LumStoreAPI.Core.Interfaces.ContentEngine;
using LumStoreAPI.Core.Interfaces.Repositories;
using LumStoreAPI.Core.Models.Enums;
using LumStoreAPI.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace LumStoreAPI.Infrastructure.Repositories.Presentations
{
    internal class EmailRepository : IEmailRepository
    {
        private readonly LumStoreContext _lumStoreContext;

        public EmailRepository(LumStoreContext lumStoreContext)
        {
            _lumStoreContext = lumStoreContext;
        }
        public Task<int> DeleteEmailQueueAsync(int id)
        {
            return DeleteEmailQueuesAsync(x => x.ItemID == id);
        }

        public Task<int> DeleteEmailQueuesAsync(Expression<Func<EmailQueue, bool>> expression)
        {
            return _lumStoreContext.EmailQueues.Where(expression).ExecuteDeleteAsync();
        }

        public Task<IPagedEnumerable<EmailQueue>> GetEmailQueuesAsync(int page, int pageSize, Expression<Func<EmailQueue, bool>>? expression = null)
        {
            return _lumStoreContext
                .EmailQueues
                .Where(expression ?? (x => true))
                .OrderBy(x => x.ItemID)
                .AsQueryable()
                .GetPagedAsync(page, pageSize);
        }

        public async Task<IEnumerable<EmailQueue>> GetEmailQueuesAsync(Func<IQueryable<EmailQueue>, IQueryable<EmailQueue>>? action = null)
        {
            IQueryable<EmailQueue> emailQueues = _lumStoreContext.EmailQueues.AsQueryable();
            if (action != null)
            {
                emailQueues = action.Invoke(emailQueues);
            }
            return (await emailQueues.ToArrayAsync()) ?? [];
        }

        public async Task<EmailQueue> InsertEmailQueueAsync(EmailQueue emailQueue)
        {
            _lumStoreContext.EmailQueues.Add(emailQueue);

            await _lumStoreContext.SaveChangesAsync();

            return emailQueue;
        }

        public Task<int> UpdateEmailAsync(int emailID, EmailStatus emailStatus)
        {
            return _lumStoreContext.EmailQueues.Where(x => x.ItemID == emailID)
                .ExecuteUpdateAsync(x => x.SetProperty(p => p.EmailStatus, emailStatus));
        }
    }
}
