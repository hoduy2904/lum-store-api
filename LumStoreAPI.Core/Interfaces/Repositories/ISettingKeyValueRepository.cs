using LumStoreAPI.Core.Entities.Systems;

namespace LumStoreAPI.Core.Interfaces.Repositories
{
    public interface ISettingKeyValueRepository
    {
        Task<SettingKeyValue> GetSettingKey(string key);
        Task<SettingKeyValue> GetSettingKeysAsync();
        Task<SettingKeyValue> InsertSettingKeyAsync(SettingKeyValue settingKeyValue);
        Task<SettingKeyValue> UpdateSettingKeyAsync(SettingKeyValue settingKeyValue);
        Task<int> DeleteSettingKeyAsync(string key);
        Task<int> DeleteSettingKeysAsync(string[] keys);
    }
}
