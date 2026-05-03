namespace LumStoreAPI.Core.Models.Systems.SettingKeys
{
    public class NotificationSettings
    {
        public bool NewOrder { get; set; } = true;
        public bool Returns { get; set; } = true;
        public bool LowStock { get; set; } = true;
        public int LowStockThreshold { get; set; } = 5;
        public bool TierUpgrade { get; set; } = true;
        public List<string> Recipients { get; set; } = [];
        public List<CustomNotificationItem> CustomNotifications { get; set; } = [];
    }

    public class CustomNotificationItem
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Label { get; set; } = default!;
        public string? Description { get; set; }
        public bool Enabled { get; set; } = true;
    }
}
