namespace LumStoreAPI.Core.Models.Systems
{
    public class AppConfiguration
    {
        public static JwtSettings JwtSettings { get; private set; } = new();
    }
}
