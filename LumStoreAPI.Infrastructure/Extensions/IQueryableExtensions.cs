using LumStoreAPI.Core.Interfaces.ContentEngine;
using LumStoreAPI.Infrastructure.Extensions;
using LumStoreAPI.Infrastructure.Types;
using LumStoreAPI.Libraries.Extensions;
using Microsoft.EntityFrameworkCore;

namespace LumStoreAPI.Infrastructure.Extensions
{
    public static class IQueryableExtensions
    {
        extension<T>(IQueryable<T> query)
        {
            public async Task<IPagedEnumerable<T>> GetPagedAsync(int page, int pageSize)
            {
                var realQuery = query;
                int count = await query.CountAsync();

                var data = await realQuery.Take(pageSize).Skip((page - 1) * pageSize).ToListAsync();

                return new PagedEnumerable<T>(data ?? [], count);
            }
        }
    }
}
