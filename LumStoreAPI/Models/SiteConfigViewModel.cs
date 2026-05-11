using LumStoreAPI.Application.DTOs.StoreDTO;
using LumStoreAPI.Core.Entities.Systems;
using LumStoreAPI.Core.Models.Systems;

namespace LumStoreAPI.Models;

public record class SiteConfigViewModel(
    IEnumerable<ContentKeyValue> LayoutSettings,
    IEnumerable<SettingKeyValue> SettingKeyValues,
    IEnumerable<NavItem> FooterColumns,
    IEnumerable<NavItem> Navigations
);
