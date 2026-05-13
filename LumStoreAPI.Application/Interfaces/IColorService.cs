using System;
using LumStoreAPI.Application.DTOs.StoreDTO;

namespace LumStoreAPI.Application.Interfaces;

public interface IColorService
{
    Task<IEnumerable<RelatedContentKeyValue>> GetGroupColorsAsync();
}
