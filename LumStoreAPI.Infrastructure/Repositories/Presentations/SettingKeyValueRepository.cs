using LumStoreAPI.Core.Entities.Systems;
using LumStoreAPI.Core.Interfaces.ContentEngine;
using LumStoreAPI.Core.Interfaces.Repositories;
using LumStoreAPI.Core.Interfaces.Sytems;
using LumStoreAPI.Core.Models.Riches;
using LumStoreAPI.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace LumStoreAPI.Infrastructure.Repositories.Presentations
{
    internal class SettingKeyValueRepository : ISettingKeyValueRepository
    {
        private readonly LumStoreContext _lumStoreContext;
        private readonly ICacheService _cacheService;
        public SettingKeyValueRepository(LumStoreContext lumStoreContext, ICacheService cacheService)
        {
            _lumStoreContext = lumStoreContext;
            _cacheService = cacheService;
        }
        public async Task<int> DeleteSettingKeyAsync(string key)
        {
            var count = await _lumStoreContext.SettingKeyValues.Where(x => x.SettingCode.Equals(key)).ExecuteDeleteAsync();
            if (count > 0)
            {
                _cacheService.TouchKey(new CacheDependency().SettingKey(key).GetDependencies().ToArray());
            }
            return count;
        }

        public async Task<int> DeleteSettingKeysAsync(string[] keys)
        {
            var count = await _lumStoreContext.SettingKeyValues.Where(x => keys.Contains(x.SettingCode)).ExecuteDeleteAsync();
            if (count > 0)
            {
                var cacheDepenencies = new CacheDependency();
                foreach (var key in keys)
                {
                    cacheDepenencies.SettingKey(key);
                }

                _cacheService.TouchKey(cacheDepenencies.GetDependencies().ToArray());
            }
            return count;
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
            _cacheService.TouchKey(new CacheDependency().SettingKeys().GetDependencies().ToArray());

            return settingKeyValue;
        }

        public async Task<SettingKeyValue> UpdateSettingKeyAsync(SettingKeyValue settingKeyValue)
        {
            _lumStoreContext.SettingKeyValues.Update(settingKeyValue);
            await _lumStoreContext.SaveChangesAsync();

            _cacheService.TouchKey(new CacheDependency().SettingKey(settingKeyValue.SettingCode).GetDependencies().ToArray());
            return settingKeyValue;
        }
    }
}
