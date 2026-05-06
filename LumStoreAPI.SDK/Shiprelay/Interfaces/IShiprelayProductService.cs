using System;
using LumStoreAPI.Core.Models.Enums;
using LumStoreAPI.SDK.Shiprelay.Models;
using LumStoreAPI.SDK.Shiprelay.Models.Product;
using LumStoreAPI.SDK.Shiprelay.Models.Requests;

namespace LumStoreAPI.SDK.Shiprelay.Interfaces;

public interface IShiprelayProductService
{

    Task<ShiprelayPagedResponse<ShiprelayProduct>?> GetShiprelayProductsAsync(ShiprelayProductGetRequest request);
    Task<bool> IsExistsProductAsync(int id);
    Task<bool> IsExistsProductAsync(string sku); 
    Task<ShiprelayProduct?> UpdateProductAsync(int id, ShiprelayProductUpdateRequest request, ProductType productType, bool ensureSuccess = true);
    Task<ShiprelayProduct?> PostProductAsync(ShiprelayProductUpdateRequest request, ProductType productType);
    Task<ShiprelayProduct?> RestoreProductAsync(int id);
    Task<ShiprelayProduct?> ArchiveProductAsync(int id);
}
