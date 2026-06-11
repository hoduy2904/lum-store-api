using LumStoreAPI.Core.Interfaces.ContentEngine;

namespace LumStoreAPI.Application.DTOs.Responses
{
    public class PaginationMeta
    {
        private readonly IEnumerable<object> _data = [];
        public int TotalRecords { get; internal set; }
        public int TotalPages => (int)Math.Ceiling(TotalRecords / (double)PageSize);
        public int TotalItems => _data.Count();
        public int PageSize { get; internal set; }
        public int CurrentPage { get; internal set; }
        public PaginationMeta()
        {

        }

        public PaginationMeta(IPagedEnumerable<object> data, int currentPage, int pageSize)
        {
            _data = data;
            this.TotalRecords = data.TotalRecords;
            this.CurrentPage = currentPage;
            this.PageSize = pageSize;

        }
    }
}
