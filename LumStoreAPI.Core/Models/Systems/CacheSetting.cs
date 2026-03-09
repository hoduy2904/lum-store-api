namespace LumStoreAPI.Core.Models.Systems
{
    public class CacheSetting
    {
        public int CacheMinutes { get; set; } = 10;
        public string CacheKey { get; set; }

        public CacheSetting(int cacheMinutes, string cacheKey)
        {
            this.CacheMinutes = cacheMinutes;
            this.CacheKey = cacheKey;
        }

        public CacheSetting(string cacheKey)
        {
            this.CacheKey = cacheKey;
        }
        public CacheSetting()
        {
            this.CacheKey = string.Empty;
        }
    }
}
