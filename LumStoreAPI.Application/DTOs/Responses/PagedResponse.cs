using LumStoreAPI.Core.Interfaces.ContentEngine;
using LumStoreAPI.Infrastructure.Extensions;

namespace LumStoreAPI.Application.DTOs.Responses
{
    public class PagedResponse<T> : APIResponseBase
    {
        public IPagedEnumerable<T> Data { get; internal set; } = Enumerable.Empty<T>().AsPagedEnumerable(0);
        public PaginationMeta Pagination { get; internal set; } = new();

        public static PagedResponse<T> Success(IPagedEnumerable<T> data, int currentPage, int pageSize, string[]? messages = null)
            => new PagedResponse<T>
            {
                Data = data,
                IsSuccess = true,
                Messages = messages ?? [],
                Pagination = new PaginationMeta(data.Cast<object>().AsPagedEnumerable(data.TotalRecords), currentPage, pageSize)
            };
    }
}
