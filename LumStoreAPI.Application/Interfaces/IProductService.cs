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

    /// <summary>Products under a category node, with raw image GUIDs (no URL resolution).</summary>
    Task<IPagedEnumerable<DocumentClientGetDTO>> GetProductsByCategoryAsync(
        int categoryNodeId, CategoryProductsRequest request);

    /// <summary>Keyed by category NodeID → published product count.</summary>
    Task<Dictionary<int, int>> GetPublishedProductCountsAsync(int[] categoryNodeIds);

    /// <summary>Real-time search suggestions matching productName, shortDescription, or SKU.</summary>
    Task<IEnumerable<SearchSuggestionDTO>> GetSearchSuggestionsAsync(string q, int limit);

    /// <summary>Product names from best-seller / new products for search recommendation chips.</summary>
    Task<IEnumerable<string>> GetSearchRecommendationsAsync(int limit);
}
