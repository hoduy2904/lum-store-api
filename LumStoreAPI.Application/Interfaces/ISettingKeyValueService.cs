using System;
using LumStoreAPI.Application.DTOs.StoreDTO;
using LumStoreAPI.Core.Entities.Systems;

namespace LumStoreAPI.Application.Interfaces;

public interface ISettingKeyValueService
{
    Task<T?> GetSystemSettingAsync<T>();
    Task<IEnumerable<ContentKeyValue>> GetSystemSettingsAsync(params string[] keys);
    Task<IEnumerable<SettingKeyValue>> GetSettingsAsync(params string[] keys);
    Task<IEnumerable<ContentKeyValue>> GetSettingContentsAsync(string startWith);
}
