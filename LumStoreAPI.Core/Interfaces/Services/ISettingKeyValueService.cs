using LumStoreAPI.Core.Entities.Systems;
using LumStoreAPI.Core.Models.Systems;

namespace LumStoreAPI.Core.Interfaces.Services;

public interface ISettingKeyValueService
{
    Task<T?> GetSystemSettingAsync<T>();
    Task<IEnumerable<ContentKeyValue>> GetSystemSettingsAsync(params string[] keys);
    Task<IEnumerable<SettingKeyValue>> GetSettingsAsync(params string[] keys);
    Task<IEnumerable<ContentKeyValue>> GetSettingContentsAsync(string startWith);
}
