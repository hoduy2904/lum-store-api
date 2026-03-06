using LumStoreAPI.Core.Entities.Systems;
using LumStoreAPI.Core.Interfaces.ContentEngine;
using LumStoreAPI.Core.Interfaces.Repositories;
using LumStoreAPI.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace LumStoreAPI.Infrastructure.Repositories.Presentations
{
    internal class SettingKeyValueRepository : ISettingKeyValueRepository
    {
        private readonly LumStoreContext _lumStoreContext;
        public SettingKeyValueRepository(LumStoreContext lumStoreContext)
        {
            _lumStoreContext = lumStoreContext;
        }
        public Task<int> DeleteSettingKeyAsync(string key)
        {
            return _lumStoreContext.SettingKeyValues.Where(x => x.SettingCode.Equals(key)).ExecuteDeleteAsync();
        }

        public Task<int> DeleteSettingKeysAsync(string[] keys)
        {
            return _lumStoreContext.SettingKeyValues.Where(x => keys.Contains(x.SettingCode)).ExecuteDeleteAsync();
        }

        public async Task<SettingKeyValue?> GetSettingKey(string key)
        {
            return await _lumStoreContext.SettingKeyValues.FindAsync(key);
        }

        public async Task<IEnumerable<SettingKeyValue>> GetSettingKeysAsync(Expression<Func<SettingKeyValue, bool>> func)
        {
            return await _lumStoreContext.SettingKeyValues.Where(func).ToArrayAsync();
        }

        public Task<IPagedEnumerable<SettingKeyValue>> GetSettingKeysAsync(int page, int pageSize, Expression<Func<SettingKeyValue, bool>> func)
        {
            return _lumStoreContext.SettingKeyValues
                .AsNoTracking()
                .Where(func)
                .OrderBy(x => x.SettingCode)
                .AsQueryable()
                .GetPagedAsync(page, pageSize);
        }

        public async Task<SettingKeyValue> InsertSettingKeyAsync(SettingKeyValue settingKeyValue)
        {
            _lumStoreContext.SettingKeyValues.Add(settingKeyValue);
            await _lumStoreContext.SaveChangesAsync();
            return settingKeyValue;
        }

        public async Task<SettingKeyValue> UpdateSettingKeyAsync(SettingKeyValue settingKeyValue)
        {
            _lumStoreContext.SettingKeyValues.Update(settingKeyValue);
            await _lumStoreContext.SaveChangesAsync();
            return settingKeyValue;
        }
    }
}
