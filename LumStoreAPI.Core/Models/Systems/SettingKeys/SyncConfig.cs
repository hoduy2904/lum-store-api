namespace LumStoreAPI.Core.Models.Systems.SettingKeys
{
    public class SyncConfig
    {
        public string Mode { get; set; } = "webhook";       // "webhook" | "scheduled" | "manual"
        public int FrequencyValue { get; set; } = 1;
        public string FrequencyUnit { get; set; } = "hour"; // "hour" | "day" | "week"
    }
}
