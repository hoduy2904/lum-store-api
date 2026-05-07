using LumStoreAPI.Core.Entities.Contact;
using LumStoreAPI.Core.Interfaces.ContentEngine;

namespace LumStoreAPI.Core.Interfaces.Repositories;

public interface IContactMessageRepository
{
    Task<ContactMessage> InsertAsync(ContactMessage message);
    Task<IPagedEnumerable<ContactMessage>> GetPagedAsync(int page, int pageSize, bool? isRead = null);
    Task<ContactMessage?> GetByIdAsync(int id);
    Task<bool> MarkAsReadAsync(int id);
}
