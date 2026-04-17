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
        };


    }
}
