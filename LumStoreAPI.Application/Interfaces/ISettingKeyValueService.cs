using System;

namespace LumStoreAPI.Application.Interfaces;

public interface ISettingKeyValueService
{
    Task<T?> GetSystemSettingAsync<T>();
    Task<IEnumerable<object>> GetSystemSettingsAsync(params string[] keys);
}
