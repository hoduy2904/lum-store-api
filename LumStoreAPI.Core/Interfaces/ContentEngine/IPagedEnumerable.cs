namespace LumStoreAPI.Core.Interfaces.ContentEngine
{
    public interface IPagedEnumerable<T> : IEnumerable<T>
    {
        public int TotalRecords { get; }
    }
}
