namespace LumStoreAPI.Core.Models.Systems.SettingKeys
{
    public class GeneralSettings
    {
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public string? LogoUrl { get; set; }
        public string? FaviconUrl { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
    }
}
