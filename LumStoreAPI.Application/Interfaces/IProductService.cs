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

    /// <summary>Find a single product that has a variant matching the given color (case-insensitive).</summary>
    Task<ProductByColorDTO?> GetProductByColorAsync(string color);

    /// <summary>
    /// Paginated list of products that have at least one variant matching the given color.
    /// Each item carries the best-matching variant (highest stock). Stable order: isBestSeller DESC, stock DESC, nodeId ASC.
    /// </summary>
    Task<IPagedEnumerable<ProductByColorItemDTO>> GetProductsByColorAsync(string color, int page, int pageSize);

    /// <summary>
    /// Up to <paramref name="limit"/> published products in the same category as the given product alias,
    /// excluding the product itself. Order: isBestSeller DESC, NodeOrder ASC.
    /// </summary>
    Task<IEnumerable<DocumentClientGetDTO>> GetRelatedProductsAsync(string nodeAlias, int limit);
}
