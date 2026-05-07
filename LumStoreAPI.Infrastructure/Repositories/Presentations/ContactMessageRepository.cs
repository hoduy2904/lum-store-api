using LumStoreAPI.Core.Entities.Contact;
using LumStoreAPI.Core.Interfaces.ContentEngine;
using LumStoreAPI.Core.Interfaces.Repositories;
using LumStoreAPI.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace LumStoreAPI.Infrastructure.Repositories.Presentations;

internal class ContactMessageRepository(LumStoreContext ctx) : IContactMessageRepository
{
    public async Task<ContactMessage> InsertAsync(ContactMessage message)
    {
        ctx.ContactMessages.Add(message);
        await ctx.SaveChangesAsync();
        return message;
    }

    public Task<IPagedEnumerable<ContactMessage>> GetPagedAsync(int page, int pageSize, bool? isRead = null)
    {
        IQueryable<ContactMessage> query = ctx.ContactMessages.AsNoTracking();

        if (isRead.HasValue)
            query = query.Where(x => x.IsRead == isRead.Value);

        return query
            .OrderByDescending(x => x.CreatedAt)
            .AsQueryable()
            .GetPagedAsync(page, pageSize);
    }

    public Task<ContactMessage?> GetByIdAsync(int id)
        => ctx.ContactMessages.FirstOrDefaultAsync(x => x.ItemID == id);

    public async Task<bool> MarkAsReadAsync(int id)
    {
        var count = await ctx.ContactMessages
            .Where(x => x.ItemID == id)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.IsRead, true));
        return count > 0;
    }
}
