using LumStoreAPI.Core.Interfaces.ContentEngine;

namespace LumStoreAPI.Application.DTOs.Responses
{
    public class PaginationMeta
    {
        public int TotalItems { get; internal set; }
        public int TotalPages => (int)Math.Ceiling(TotalItems / (double)PageSize);
        public int PageSize { get; internal set; }
        public int CurrentPage { get; internal set; }
        public PaginationMeta()
        {

        }

        public PaginationMeta(IPagedEnumerable<object> data, int currentPage, int pageSize)
        {
            this.TotalItems = data.TotalRecords;
            this.CurrentPage = currentPage;
            this.PageSize = pageSize;
        }
    }
}
