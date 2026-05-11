using System;
using LumStoreAPI.Core.Models.Systems;

namespace LumStoreAPI.Application.Interfaces;

public interface ISiteService
{
    Task<IEnumerable<NavItem>> GetNavigationsAsync();
}
