using System;
using LumStoreAPI.Application.DTOs.DocumentPageDTO;
using LumStoreAPI.Application.DTOs.StoreDTO;
using LumStoreAPI.Core.Entities.Systems;

namespace LumStoreAPI.Models;

public record class SiteConfigViewModel(
    IEnumerable<ContentKeyValue> LayoutSettings,
    IEnumerable<SettingKeyValue> SettingKeyValues,
    IEnumerable<DocumentClientGetLinkedDTO> FooterColumns,
    IEnumerable<DocumentClientGetLinkedDTO> Navigations
);
