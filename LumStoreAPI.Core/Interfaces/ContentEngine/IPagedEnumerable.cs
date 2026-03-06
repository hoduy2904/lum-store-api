namespace LumStoreAPI.Core.Interfaces.ContentEngine
{
    public interface IPagedEnumerable<T> : IEnumerable<T>
    {
        public int TotalRecords { get; }

        public IPagedEnumerable<TResult> Select<TResult>(Func<T, TResult> selector);
    }
}
