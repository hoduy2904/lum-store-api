using System.Text.Json;

namespace LumStoreAPI.Application.DTOs.Systems
{
    public class SettingKeySystemRequest
    {
        public string SettingCode { get; set; } = default!;
        public string SettingName { get; set; } = default!;
        public JsonElement SettingValue { get; set; }
    }
}
