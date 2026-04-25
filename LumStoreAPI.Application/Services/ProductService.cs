using LumStoreAPI.Application.DTOs.DocumentPageDTO;
using LumStoreAPI.Application.DTOs.ProductDTO;
using LumStoreAPI.Application.DTOs.ProductVariantDTO;
using LumStoreAPI.Application.DTOs.StoreDTO;
using LumStoreAPI.Application.Interfaces;
using LumStoreAPI.Core.Entities.Pages;
using LumStoreAPI.Core.Interfaces.ContentEngine;
using LumStoreAPI.Core.Interfaces.Repositories;
using LumStoreAPI.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;


namespace LumStoreAPI.Application.Services;

internal class ProductService
(
IPageRetrieveContext pageRetrieveContext,
LumStoreContext lumStoreContext,
IMediaService mediaService,
IProductVariantRepository productVariantRepository)
 : IProductService
{
    private readonly IPageRetrieveContext _pageRetrieveContext = pageRetrieveContext;
    private readonly IMediaService _mediaService = mediaService;
    private readonly LumStoreContext _lumStoreContext = lumStoreContext;
    private readonly IProductVariantRepository _productVariantRepository = productVariantRepository;
    public Task<IEnumerable<DocumentClientGetDTO>> GetFeatureProducts(int topN)
    {
        return this.GetProducts(x => x.IsBestSeller, topN);
    }

    public Task<IEnumerable<DocumentClientGetDTO>> GetNewProducts(int topN)
    {
        var sevenDaysAgo = DateTime.UtcNow.Date.AddDays(-7);
        return this.GetProducts(x => x.CreatedAt >= sevenDaysAgo, topN);
    }

    public async Task<IEnumerable<StoreCategoryDTO>> GetProductCategories()
    {
        var categories = await _lumStoreContext.ProductCategories
          .GroupBy(x => new { x.CategoryName, x.NodeID, x.Node.RelativeUrl })
          .Select(x => new StoreCategoryDTO
          {
              Name = x.Key.CategoryName,
              Slug = x.Key.RelativeUrl,
              Id = x.Key.NodeID,
              ProductCount = x.Count()
          })
          .ToListAsync();

        return categories;
    }

    public async Task<IPagedEnumerable<DocumentClientGetDTO>> GetProducts(ProductClientRequestDTO request)
    {
        var products = await this.GetProducts(request.Page, request.PageSize, request.CategoryId, query =>
        (string.IsNullOrEmpty(request.Color) || query.ProductVariants.Any(p => p.VariantName.Equals(request.Color)))
        && (string.IsNullOrWhiteSpace(request.Search) || query.ProductName.Contains(request.Search))
        );

        return products;
    }

    private async Task<IPagedEnumerable<DocumentClientGetDTO>> GetProducts(int page, int pageSize, int categoryId, Expression<Func<Product, bool>> where)
    {
        var products = await _pageRetrieveContext.GetPagedPagesAsync<Product>(query =>
         {
             query.Where(where)
             .Paged(page, pageSize)
             .Select(x => new Product
             {
                 CreatedAt = x.CreatedAt,
                 Height = x.Height,
                 Images = x.Images,
                 IsAlcoholic = x.IsAlcoholic,
                 DocumentName = x.DocumentName,
                 IsBestSeller = x.IsBestSeller,
                 IsFoldable = x.IsFoldable,
                 IsNeedBox = x.IsNeedBox,
                 IsFragile = x.IsFragile,
                 IsHazmat = x.IsHazmat,
                 Length = x.Length,
                 ProductName = x.ProductName,
                 PublishedFrom = x.PublishedFrom,
                 PublishedTo = x.PublishedTo,
                 Width = x.Width,
                 NodeID = x.NodeID,
                 PageID = x.PageID,
                 ShortDescription = x.ShortDescription,
                 Weight = x.Weight,
                 Node = x.Node,
                 UpdatedAt = x.UpdatedAt,
                 Price = x.Price,
                 PriceDiscount = x.PriceDiscount,
             });

             if (categoryId != 0)
             {
                 query.GetDescendants(categoryId);
             }
         });

        var imageGuids = products.SelectMany(x => x.Images).ToArray();
        var images = await _mediaService.GetMediaItemsAsync(imageGuids);
        var variantsByProduct = await LoadVariantsAsync(products.Select(x => x.NodeID));
        var productDTOs = products.Select(x =>
        {
            var productItem = new ProductClientDTO(x)
            {
                Images = images.Where(i => x.Images.Contains(i.FileID)).Select(i => i.FileURL).ToArray(),
                ProductVariants = variantsByProduct.GetValueOrDefault(x.NodeID, [])
            };

            var product = new DocumentClientGetDTO(productItem, x);
            return product;
        });

        return productDTOs;
    }

    private async Task<IEnumerable<DocumentClientGetDTO>> GetProducts(Expression<Func<Product, bool>> where, int topN)
    {
        if (topN > 20)
        {
            topN = 20;
        }
        var products = await _pageRetrieveContext.GetPagesAsync<Product>(query =>
         {
             query.Where(where)
             .IncludeQueryable(x => x.Take(topN))
             .Select(x => new Product
             {
                 CreatedAt = x.CreatedAt,
                 Height = x.Height,
                 Images = x.Images,
                 IsAlcoholic = x.IsAlcoholic,
                 DocumentName = x.DocumentName,
                 IsBestSeller = x.IsBestSeller,
                 IsFoldable = x.IsFoldable,
                 IsNeedBox = x.IsNeedBox,
                 IsFragile = x.IsFragile,
                 IsHazmat = x.IsHazmat,
                 Length = x.Length,
                 ProductName = x.ProductName,
                 PublishedFrom = x.PublishedFrom,
                 PublishedTo = x.PublishedTo,
                 Width = x.Width,
                 NodeID = x.NodeID,
                 PageID = x.PageID,
                 ShortDescription = x.ShortDescription,
                 Weight = x.Weight,
                 Node = x.Node,
                 UpdatedAt = x.UpdatedAt,
                 Price = x.Price,
                 PriceDiscount = x.PriceDiscount,
             });
         });

        var productList = products.ToList();
        var imageGuids = productList.SelectMany(x => x.Images).ToArray();
        var images = await _mediaService.GetMediaItemsAsync(imageGuids);
        var variantsByProduct = await LoadVariantsAsync(productList.Select(x => x.NodeID));
        var productDTOs = productList.Select(x =>
        {
            var productItem = new ProductClientDTO(x)
            {
                Images = images.Where(i => x.Images.Contains(i.FileID)).Select(i => i.FileURL).ToArray(),
                ProductVariants = variantsByProduct.GetValueOrDefault(x.NodeID, [])
            };

            var product = new DocumentClientGetDTO(productItem, x);
            return product;
        });

        return productDTOs;
    }

    public Task<IEnumerable<DocumentClientGetDTO>> GetProductsByNodeIdsAsync(int[] nodeIds)
    {
        if (nodeIds.Length == 0) return Task.FromResult(Enumerable.Empty<DocumentClientGetDTO>());
        return GetProducts(x => nodeIds.Contains(x.NodeID), nodeIds.Length);
    }

    public async Task<IPagedEnumerable<DocumentClientGetDTO>> GetProductsByCategoryAsync(
        int categoryNodeId, CategoryProductsRequest request)
    {
        var products = await _pageRetrieveContext.GetPagedPagesAsync<Product>(query =>
        {
            query
                .GetDescendants(categoryNodeId, 1)
                .Published(Core.Models.Enums.TreeNodePublished.Published)
                .Where(x =>
                    (!request.MinPrice.HasValue || x.Price >= request.MinPrice.Value) &&
                    (!request.MaxPrice.HasValue || x.Price <= request.MaxPrice.Value))
                .Paged(request.Page, request.PageSize)
                .IncludeQueryable(q => request.SortBy switch
                {
                    "price_asc" => q.OrderBy(x => x.Price),
                    "price_desc" => q.OrderByDescending(x => x.Price),
                    "best_sellers" => q.OrderByDescending(x => x.IsBestSeller).ThenByDescending(x => x.Node.NodeOrder),
                    _ => q.OrderByDescending(x => x.Node.NodeOrder) // newest
                });
        });

        var imageGuids = products.SelectMany(x => x.Images).Distinct().ToArray();
        var productImages = imageGuids.Length > 0
            ? (await _mediaService.GetMediaItemsAsync(imageGuids)).ToList()
            : [];

        var variantsByProduct = await LoadVariantsRawAsync(products.Select(x => x.NodeID));

        return products.Select(p =>
        {
            var variants = variantsByProduct.GetValueOrDefault(p.NodeID, []);
            var fields = new CategoryProductFieldsDTO
            {
                ProductName = p.ProductName,
                ShortDescription = p.ShortDescription,
                Description = p.Description,
                IsBestSeller = p.IsBestSeller,
                Price = p.Price,
                PriceDiscount = p.PriceDiscount,
                Images = productImages
                    .Where(img => p.Images.Contains(img.FileID))
                    .Select(img => img.FileURL)
                    .ToArray(),
                Stock = variants.Sum(v => v.Stock),
                ProductVariants = variants
            };
            return new DocumentClientGetDTO(fields, p);
        });
    }

    public async Task<Dictionary<int, int>> GetPublishedProductCountsAsync(int[] categoryNodeIds)
    {
        if (categoryNodeIds.Length == 0) return [];

        var now = DateTimeOffset.UtcNow;
        return await _lumStoreContext.Products
            .Where(p =>
                p.Node.ParentNodeID.HasValue &&
                categoryNodeIds.Contains(p.Node.ParentNodeID.Value) &&
                !p.IsDeleted &&
                (p.PublishedFrom == null || p.PublishedFrom <= now) &&
                (p.PublishedTo == null || p.PublishedTo > now))
            .GroupBy(p => p.Node.ParentNodeID!.Value)
            .Select(g => new { NodeId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.NodeId, x => x.Count);
    }

    /// <summary>
    /// Batch-loads variants and resolves their image GUIDs to URLs in one media call.
    /// Returns a dictionary keyed by ProductID → list of CategoryProductVariantDTO.
    /// </summary>
    private async Task<Dictionary<int, List<CategoryProductVariantDTO>>> LoadVariantsRawAsync(IEnumerable<int> productIds)
    {
        var ids = productIds.ToList();
        if (ids.Count == 0) return [];

        var variants = (await _productVariantRepository.GetProductVariantsAsync(v => ids.Contains(v.ProductID))).ToList();
        if (variants.Count == 0) return [];

        var variantImageGuids = variants.SelectMany(v => v.Images).Distinct().ToArray();
        var variantImages = variantImageGuids.Length > 0
            ? (await _mediaService.GetMediaItemsAsync(variantImageGuids)).ToList()
            : [];

        return variants
            .GroupBy(v => v.ProductID)
            .ToDictionary(
                g => g.Key,
                g => g.Select(v => new CategoryProductVariantDTO
                {
                    VariantId = v.ItemID,
                    VariantName = v.VariantName,
                    Color = v.Color,
                    Stock = v.Stock,
                    SKU = v.SKU,
                    Images = variantImages
                        .Where(img => v.Images.Contains(img.FileID))
                        .Select(img => img.FileURL)
                        .ToArray()
                }).ToList()
            );
    }

    /// <summary>
    /// Batch-loads variants for a set of product IDs and resolves their images in one media call.
    /// Returns a dictionary keyed by ProductID → list of mapped DTOs.
    /// </summary>
    private async Task<Dictionary<int, List<ProductVariantGetDTO>>> LoadVariantsAsync(IEnumerable<int> productIds)
    {
        var ids = productIds.ToList();
        if (ids.Count == 0)
            return [];

        var variants = (await _productVariantRepository.GetProductVariantsAsync(v => ids.Contains(v.ProductID))).ToList();
        if (variants.Count == 0)
            return [];

        var variantImageGuids = variants.SelectMany(v => v.Images).Distinct().ToArray();
        var variantImages = variantImageGuids.Length > 0
            ? (await _mediaService.GetMediaItemsAsync(variantImageGuids)).ToList()
            : [];

        return variants
            .GroupBy(v => v.ProductID)
            .ToDictionary(
                g => g.Key,
                g => g.Select(v => new ProductVariantGetDTO(
                    v,
                    variantImages.Where(img => v.Images.Contains(img.FileID)).ToArray()
                )).ToList()
            );
    }
}
