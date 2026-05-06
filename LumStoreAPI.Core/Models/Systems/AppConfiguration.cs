namespace LumStoreAPI.Core.Models.Systems
{
    public class AppConfiguration
    {
        public static AdminConfiguration AdminConfiguration { get; private set; } = new();
    }
}
