using LumStoreAPI.Core.Entities.DocumentTypes;
using LumStoreAPI.Core.Entities.Integrations;
using LumStoreAPI.Core.Interfaces.ContentEngine;
using LumStoreAPI.Core.Models.Enums;
using System.Linq.Expressions;

namespace LumStoreAPI.Core.Interfaces.Repositories
{
    public interface IShiprelaySystemRespository
    {
        Task SyncProductShiprelayAsync(int productId, EntryActionStatus entryActionStatus, int variantId);
        Task SyncVariantShiprelayAsync(int variantId, EntryActionStatus entryActionStatus);
        Task SyncProductShiprelaysAsync(int[] productIds, EntryActionStatus entryActionStatus);
        Task SyncVariantShiprelayAsync(ProductVariant[] variants, EntryActionStatus entryActionStatus);
        Task SyncVariantShiprelayAsync(int[] variantIds, EntryActionStatus entryActionStatus);
        Task<IPagedEnumerable<ShiprelayDataSync>> GetShiprelayDataAsync(int page, int pageSize, Expression<Func<ShiprelayDataSync, bool>>? where = null);
        Task<bool> UpdateShiprelayDataAsync(int productId, int? variantId, EmailStatus status, EntryActionStatus entryActionStatus, string? message = null);
        Task<int> DeleteShiprelayDataAsync(int itemId);
        Task<bool> UpdateShiprelayDataAsync(int itemId, EmailStatus status, EntryActionStatus entryActionStatus, string? message = null);
    }
}
