using System;
using LumStoreAPI.Application.DTOs.DocumentPageDTO;
using LumStoreAPI.Application.DTOs.ProductDTO;
using LumStoreAPI.Application.DTOs.StoreDTO;
using LumStoreAPI.Core.Interfaces.ContentEngine;

namespace LumStoreAPI.Application.Interfaces;

public interface IProductService
{
    Task<IEnumerable<DocumentClientGetDTO>> GetFeatureProducts(int topN);
    Task<IEnumerable<DocumentClientGetDTO>> GetNewProducts(int topN);
    Task<IPagedEnumerable<DocumentClientGetDTO>> GetProducts(ProductClientRequestDTO request);
    Task<IEnumerable<StoreCategoryDTO>> GetProductCategories();
    Task<IEnumerable<DocumentClientGetDTO>> GetProductsByNodeIdsAsync(int[] nodeIds);
}
