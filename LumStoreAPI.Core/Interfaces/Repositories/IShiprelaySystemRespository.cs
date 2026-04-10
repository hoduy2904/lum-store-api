using LumStoreAPI.Core.Entities.Integrations;
using LumStoreAPI.Core.Entities.Pages;
using LumStoreAPI.Core.Interfaces.ContentEngine;
using LumStoreAPI.Core.Models.Enums;
using System.Linq.Expressions;

namespace LumStoreAPI.Core.Interfaces.Repositories
{
    public interface IShiprelaySystemRespository
    {
        Task SyncProductShiprelayAsync(Product product);
        Task SyncProductShiprelayAsync(int productId, int? variantId = null);
        Task SyncVariantShiprelayAsync(int variantId);
        Task<IPagedEnumerable<ShiprelayDataSync>> GetShiprelayDataAsync(int page, int pageSize, Expression<Func<ShiprelayDataSync, bool>>? where = null);
        Task<bool> UpdateShiprelayDataAsync(int productId, int? variantId, EmailStatus status, string? message = null);
        Task<int> DeleteShiprelayDataAsync(int previousDay = 30);
    }
}
