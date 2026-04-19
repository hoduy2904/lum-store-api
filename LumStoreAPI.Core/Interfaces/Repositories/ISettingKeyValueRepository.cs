using LumStoreAPI.Core.Entities.Systems;
using LumStoreAPI.Core.Interfaces.ContentEngine;
using System.Linq.Expressions;

namespace LumStoreAPI.Core.Interfaces.Repositories
{
    public interface ISettingKeyValueRepository
    {
        Task<SettingKeyValue?> GetSettingKeyAsync(string key);
        Task<IEnumerable<SettingKeyValue>> GetSettingKeysAsync(Expression<Func<SettingKeyValue, bool>> func);
        Task<IPagedEnumerable<SettingKeyValue>> GetSettingKeysAsync(int page, int pageSize, Expression<Func<SettingKeyValue, bool>> func);
        Task<SettingKeyValue> InsertSettingKeyAsync(SettingKeyValue settingKeyValue);
        Task<SettingKeyValue?> UpdateSettingKeyAsync(string settingCode, SettingKeyValue settingKeyValue);
        Task<int> DeleteSettingKeyAsync(string key);
        Task<int> DeleteSettingKeysAsync(string[] keys);
    }
}
