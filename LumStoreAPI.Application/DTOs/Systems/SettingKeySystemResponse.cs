namespace LumStoreAPI.Application.DTOs.Systems
{
    public class SettingKeySystemResponse
    {
        public string SettingCode { get; set; } = default!;
        public string SettingName { get; set; } = default!;
        public object? SettingValue { get; set; }
    }
}
