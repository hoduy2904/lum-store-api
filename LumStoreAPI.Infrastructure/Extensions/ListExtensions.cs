using LumStoreAPI.Core.Interfaces.ContentEngine;
using LumStoreAPI.Infrastructure.Types;

namespace LumStoreAPI.Infrastructure.Extensions
{
    public static class ListExtensions
    {
        extension<T>(IEnumerable<T> items)
        {
            public IPagedEnumerable<T> AsPagedEnumerable(int totalRecords)
            {
                return new PagedEnumerable<T>(items, totalRecords);
            }
        }
    }
}
