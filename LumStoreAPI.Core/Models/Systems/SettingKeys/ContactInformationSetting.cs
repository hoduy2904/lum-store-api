using LumStoreAPI.Core.Models.Enums;

namespace LumStoreAPI.Core.Models.Systems.SettingKeys
{
    public class ContactInformationSetting
    {
        public ContactInfoType Type { get; set; }
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public string? IconName { get; set; }
    }
}
