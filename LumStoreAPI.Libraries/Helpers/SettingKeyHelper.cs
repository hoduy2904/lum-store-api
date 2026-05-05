using LumStoreAPI.Core.Models.Constants.Systems;
using LumStoreAPI.Core.Models.Systems.SettingKeys;

namespace LumStoreAPI.Libraries.Helpers
{
    public class SettingKeyHelper
    {
        public static Dictionary<string, Type> SystemSettingTypeMapping = new Dictionary<string, Type>()
        {
            {SystemSettingKeyConstants.SYSTEM_STORE_HOURS, typeof(IEnumerable<StoreHoursSetting>) },
            {SystemSettingKeyConstants.CONTACT_INFORMATION, typeof(IEnumerable<ContactInformationSetting>) },
            {SystemSettingKeyConstants.GENERAL_SETTINGS, typeof(GeneralSettings) },
            {SystemSettingKeyConstants.EMAIL_SETTINGS, typeof(EmailSettings) },
            {SystemSettingKeyConstants.NOTIFICATION_SETTINGS, typeof(NotificationSettings) },
            {SystemSettingKeyConstants.SYNC_CONFIG, typeof(SyncConfig) },
        };


    }
}
