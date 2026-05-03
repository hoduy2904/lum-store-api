namespace LumStoreAPI.Core.Models.Systems.SettingKeys
{
    public class StoreHoursSetting
    {
        public string Label { get; set; } = default!;
        public TimeOnly? OpenAt { get; set; }
        public TimeOnly? ClosedAt { get; set; }
        public bool IsClosed { get; set; }
    }
}
