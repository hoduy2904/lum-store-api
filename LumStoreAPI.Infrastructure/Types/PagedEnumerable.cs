using LumStoreAPI.Core.Interfaces.ContentEngine;
using System.Collections;

namespace LumStoreAPI.Infrastructure.Types
{
    internal class PagedEnumerable<T> : IPagedEnumerable<T>
    {
        private IEnumerable<T> _items;
        public int TotalRecords { get; internal set; } = 0;

        public PagedEnumerable(IEnumerable<T> items, int totalRecords)
        {
            _items = items;
            this.TotalRecords = totalRecords;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IPagedEnumerable<TResult> Select<TResult>(Func<T, TResult> selector)
        {
            return new PagedEnumerable<TResult>(_items.Select(selector), this.TotalRecords);
        }
    }
}
