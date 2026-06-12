using LumStoreAPI.Application.DTOs.DocumentPageDTO;
using LumStoreAPI.Application.DTOs.ProductComboDTO;
using LumStoreAPI.Application.DTOs.ProductDTO;
using LumStoreAPI.Application.DTOs.ProductVariantDTO;
using LumStoreAPI.Application.DTOs.StoreDTO;
using LumStoreAPI.Application.Interfaces;
using LumStoreAPI.Core.Entities.Pages;
using LumStoreAPI.Core.Interfaces.ContentEngine;
using LumStoreAPI.Core.Interfaces.Repositories;
using LumStoreAPI.Infrastructure;
using LumStoreAPI.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;


namespace LumStoreAPI.Application.Services;

internal class ProductService
(
IPageRetrieveContext pageRetrieveContext,
LumStoreContext lumStoreContext,
IMediaService mediaService,
IProductVariantRepository productVariantRepository,
IDiscountRuleService discountRuleService)
 : IProductService
{
    private readonly IPageRetrieveContext _pageRetrieveContext = pageRetrieveContext;
    private readonly IMediaService _mediaService = mediaService;
    private readonly LumStoreContext _lumStoreContext = lumStoreContext;
    private readonly IProductVariantRepository _productVariantRepository = productVariantRepository;
    private readonly IDiscountRuleService _discountRuleService = discountRuleService;

    public async Task<IEnumerable<DocumentClientGetDTO>> GetFeatureProducts(int topN)
    {
        var result = (await this.GetProducts(x => x.IsBestSeller, topN)).ToList();
        if (result.Count < topN)
        {
            var existingIds = result.Select(x => x.NodeID).ToHashSet();
            var fallback = await this.GetProducts(x => !existingIds.Contains(x.NodeID), topN - result.Count, orderByLatest: true);
            result.AddRange(fallback);
        }
        return result;
    }

    public async Task<IEnumerable<DocumentClientGetDTO>> GetNewProducts(int topN)
    {
        var sevenDaysAgo = DateTime.UtcNow.Date.AddDays(-7);
        var result = (await this.GetProducts(x => x.CreatedAt >= sevenDaysAgo, topN)).ToList();
        if (result.Count < topN)
        {
            var existingIds = result.Select(x => x.NodeID).ToHashSet();
            var fallback = await this.GetProducts(x => !existingIds.Contains(x.NodeID), topN - result.Count, orderByLatest: true);
            result.AddRange(fallback);
        }
        return result;
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
             .Where(x => x.ProductVariants.Any(v => v.ShiprelayId > 0))
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
                 IsCombo = x.IsCombo,
             });

             if (categoryId != 0)
             {
                 query.GetDescendants(categoryId);
             }
         });

        var nodeIdsArray = products.Select(x => x.NodeID).ToArray();
        var imageGuids = products.SelectMany(x => x.Images).ToArray();
        var images = await _mediaService.GetMediaItemsAsync(imageGuids);
        var variantsByProduct = await LoadVariantsAsync(products.Select(x => x.NodeID));
        var comboPrices = await GetComboPricesAsync(products);
        var discountTiers = await _discountRuleService.GetDiscountTiersForProductsAsync(nodeIdsArray);

        var productDTOs = products.Select(x =>
        {
            var productItem = new ProductClientDTO(x)
            {
                Images = images.Where(i => x.Images.Contains(i.FileID)).OrderBy(i => x.Images.IndexOf(i.FileID)).Select(i => i.FileURL).ToArray(),
                ProductVariants = variantsByProduct.GetValueOrDefault(x.NodeID, []),
                IsCombo = x.IsCombo,
                DiscountRules = discountTiers.GetValueOrDefault(x.NodeID, []),
            };

            if (x.IsCombo && comboPrices.TryGetValue(x.NodeID, out var cp))
            {
                productItem.Price = cp.TotalPrice;
                productItem.PriceDiscount = null;
                productItem.ComboStock = cp.ComboStock;
            }

            var product = new DocumentClientGetDTO(productItem, x);
            return product;
        });

        return productDTOs;
    }

    private async Task<IEnumerable<DocumentClientGetDTO>> GetProducts(Expression<Func<Product, bool>> where, int topN, bool orderByLatest = false)
    {
        if (topN > 20)
        {
            topN = 20;
        }
        var products = await _pageRetrieveContext.GetPagesAsync<Product>(query =>
         {
             query.Where(where)
             .Where(x => x.ProductVariants.Any(v => v.ShiprelayId > 0))
             .IncludeQueryable(x => orderByLatest
                 ? x.OrderByDescending(p => p.CreatedAt).Take(topN)
                 : x.Take(topN))
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
                 IsCombo = x.IsCombo,
             });
         });

        var productList = products.ToList();
        var nodeIdsArray = productList.Select(x => x.NodeID).ToArray();
        var imageGuids = productList.SelectMany(x => x.Images).ToArray();
        var images = await _mediaService.GetMediaItemsAsync(imageGuids);
        var variantsByProduct = await LoadVariantsAsync(productList.Select(x => x.NodeID));
        var comboPrices = await GetComboPricesAsync(productList);
        var discountTiers = await _discountRuleService.GetDiscountTiersForProductsAsync(nodeIdsArray);

        var productDTOs = productList.Select(x =>
        {
            var productItem = new ProductClientDTO(x)
            {
                Images = images.Where(i => x.Images.Contains(i.FileID)).OrderBy(i => x.Images.IndexOf(i.FileID)).Select(i => i.FileURL).ToArray(),
                ProductVariants = variantsByProduct.GetValueOrDefault(x.NodeID, []),
                IsCombo = x.IsCombo,
                DiscountRules = discountTiers.GetValueOrDefault(x.NodeID, []),
            };

            if (x.IsCombo && comboPrices.TryGetValue(x.NodeID, out var cp))
            {
                productItem.Price = cp.TotalPrice;
                productItem.PriceDiscount = null;
                productItem.ComboStock = cp.ComboStock;
            }

            var product = new DocumentClientGetDTO(productItem, x);
            return product;
        });

        return productDTOs;
    }

    public async Task<IEnumerable<DocumentClientGetDTO>> GetProductsByNodeIdsAsync(int[] nodeIds)
    {
        if (nodeIds.Length == 0) return Enumerable.Empty<DocumentClientGetDTO>();

        var products = (await _pageRetrieveContext.GetPagesAsync<Product>(query =>
        {
            query.Where(x => x.ProductVariants.Any(v => v.ShiprelayId > 0) && nodeIds.Contains(x.NodeID));
        })).ToList();

        if (products.Count == 0) return Enumerable.Empty<DocumentClientGetDTO>();

        var imageGuids = products.SelectMany(x => x.Images).ToArray();
        var images = await _mediaService.GetMediaItemsAsync(imageGuids);
        var variantsByProduct = await LoadVariantsAsync(products.Select(x => x.NodeID));
        var comboPrices = await GetComboPricesAsync(products);
        var discountTiers = await _discountRuleService.GetDiscountTiersForProductsAsync(nodeIds);

        return products.Select(x =>
        {
            var productItem = new ProductClientDTO(x)
            {
                Images = images.Where(i => x.Images.Contains(i.FileID)).OrderBy(i => x.Images.IndexOf(i.FileID)).Select(i => i.FileURL).ToArray(),
                ProductVariants = variantsByProduct.GetValueOrDefault(x.NodeID, []),
                IsCombo = x.IsCombo,
                DiscountRules = discountTiers.GetValueOrDefault(x.NodeID, []),
            };

            if (x.IsCombo && comboPrices.TryGetValue(x.NodeID, out var cp))
            {
                productItem.Price = cp.TotalPrice;
                productItem.PriceDiscount = null;
                productItem.ComboStock = cp.ComboStock;
            }

            return new DocumentClientGetDTO(productItem, x);
        });
    }

    public async Task<IPagedEnumerable<DocumentClientGetDTO>> GetProductsByCategoryAsync(
        int categoryNodeId, CategoryProductsRequest request)
    {
        var products = await _pageRetrieveContext.GetPagedPagesAsync<Product>(query =>
        {
            query
                .GetDescendants(categoryNodeId)
                .Published(Core.Models.Enums.TreeNodePublished.Published)
                .Where(x =>
                    (!request.MinPrice.HasValue || x.Price >= request.MinPrice.Value) &&
                    (!request.MaxPrice.HasValue || x.Price <= request.MaxPrice.Value) &&
                    x.ProductVariants.Any(v => v.ShiprelayId > 0))
                .Paged(request.Page, request.PageSize)
                .IncludeQueryable(q => request.SortBy switch
                {
                    "price_asc" => q.OrderBy(x => x.Price),
                    "price_desc" => q.OrderByDescending(x => x.Price),
                    "best_sellers" => q.OrderByDescending(x => x.IsBestSeller).ThenByDescending(x => x.Node.NodeOrder),
                    _ => q.OrderByDescending(x => x.Node.NodeOrder) // newest
                });
        });

        var categoryNodeIds = products.Select(x => x.NodeID).ToArray();
        var imageGuids = products.SelectMany(x => x.Images).Distinct().ToArray();
        var productImages = imageGuids.Length > 0
            ? (await _mediaService.GetMediaItemsAsync(imageGuids)).ToList()
            : [];

        var variantsByProduct = await LoadVariantsRawAsync(products.Select(x => x.NodeID));
        var comboPrices = await GetComboPricesAsync(products);
        var discountTiers = await _discountRuleService.GetDiscountTiersForProductsAsync(categoryNodeIds);

        return products.Select(p =>
        {
            var variants = variantsByProduct.GetValueOrDefault(p.NodeID, []);
            var productImageUrls = productImages
                .Where(img => p.Images.Contains(img.FileID))
                .OrderBy(img => p.Images.IndexOf(img.FileID))
                .Select(img => img.FileURL)
                .ToArray();

            var cp = p.IsCombo && comboPrices.TryGetValue(p.NodeID, out var cpResult) ? cpResult : null;
            decimal price = cp?.TotalPrice ?? p.Price;
            decimal priceDiscount = p.IsCombo ? 0 : p.PriceDiscount;

            var fields = new CategoryProductFieldsDTO
            {
                ProductName = p.ProductName,
                ShortDescription = p.ShortDescription,
                Description = p.Description,
                IsBestSeller = p.IsBestSeller,
                IsCombo = p.IsCombo,
                Price = price,
                PriceDiscount = priceDiscount,
                Images = productImageUrls,
                Stock = variants.Sum(v => v.Stock),
                ComboStock = cp?.ComboStock ?? 0,
                ProductVariants = variants.Select(v => new CategoryProductVariantDTO
                {
                    VariantId = v.VariantId,
                    VariantName = v.VariantName,
                    Color = v.Color,
                    Stock = v.Stock,
                    SKU = v.SKU,
                    Images = [.. productImageUrls, .. v.Images]
                }).ToList(),
                DiscountRules = discountTiers.GetValueOrDefault(p.NodeID, []),
            };
            return new DocumentClientGetDTO(fields, p);
        });
    }

    public async Task<Dictionary<int, int>> GetPublishedProductCountsAsync(int[] categoryNodeIds)
    {
        if (categoryNodeIds.Length == 0) return [];

        var now = DateTimeOffset.UtcNow;
        return await _lumStoreContext.DocumentLinkedNodes
            .Where(ln => categoryNodeIds.Contains(ln.Ancestor) && ln.Depth > 0)
            .Join(
                _lumStoreContext.Products.Where(p =>
                    !p.IsDeleted &&
                    (p.PublishedFrom == null || p.PublishedFrom <= now) &&
                    (p.PublishedTo == null || p.PublishedTo > now) &&
                    p.ProductVariants.Any(v => v.ShiprelayId > 0)),
                ln => ln.Descendant,
                p => p.NodeID,
                (ln, p) => new { ln.Ancestor })
            .GroupBy(x => x.Ancestor)
            .Select(g => new { NodeId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.NodeId, x => x.Count);
    }

    public async Task<IEnumerable<SearchSuggestionDTO>> GetSearchSuggestionsAsync(string q, int limit)
    {
        if (string.IsNullOrWhiteSpace(q)) return [];
        limit = Math.Min(limit, 10);

        var now = DateTimeOffset.UtcNow;

        var skuMatchIds = await _lumStoreContext.ProductVariants
            .Where(v => v.SKU.Contains(q))
            .Select(v => v.ProductID)
            .Distinct()
            .ToListAsync();

        var raw = await _lumStoreContext.Products
            .Where(p =>
                !p.IsDeleted &&
                (p.PublishedFrom == null || p.PublishedFrom <= now) &&
                (p.PublishedTo == null || p.PublishedTo > now) &&
                p.ProductVariants.Any(v => v.ShiprelayId > 0) &&
                (p.ProductName.Contains(q) ||
                 (p.ShortDescription != null && p.ShortDescription.Contains(q)) ||
                 skuMatchIds.Contains(p.NodeID)))
            .Select(p => new
            {
                p.NodeID,
                p.Node.NodeAlias,
                p.Node.RelativeUrl,
                p.ProductName,
                p.Price,
                p.PriceDiscount,
                p.Images,
                p.IsCombo,
                ParentNodeID = p.Node.ParentNodeID,
            })
            .Take(200)  // cap DB load before in-memory rerank
            .ToListAsync();

        var sorted = raw
            .OrderByDescending(p => p.ProductName.Equals(q, StringComparison.OrdinalIgnoreCase))
            .ThenByDescending(p => p.ProductName.StartsWith(q, StringComparison.OrdinalIgnoreCase))
            .Take(limit)
            .ToList();

        if (sorted.Count == 0) return [];

        // Batch-compute combo prices — 2 DB queries total regardless of combo count
        var comboIds = sorted.Where(p => p.IsCombo).Select(p => p.NodeID).Distinct().ToArray();
        var comboResults = comboIds.Length > 0
            ? await _discountRuleService.CalculateBatchComboPricesAsync(comboIds)
            : new Dictionary<int, ComboPriceResult>();
        var comboPriceMap = comboResults.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.TotalPrice);

        var allGuids = sorted.Where(p => p.Images.Length > 0).Select(p => p.Images[0]).Distinct().ToArray();
        var mediaMap = allGuids.Length > 0
            ? (await _mediaService.GetMediaItemsAsync(allGuids)).ToDictionary(m => m.FileID, m => m.FileURL)
            : [];

        var parentIds = sorted.Where(p => p.ParentNodeID.HasValue).Select(p => p.ParentNodeID!.Value).Distinct().ToArray();
        var categoryMap = parentIds.Length > 0
            ? await _lumStoreContext.ProductCategories
                .Where(c => parentIds.Contains(c.NodeID))
                .ToDictionaryAsync(c => c.NodeID, c => c.CategoryName)
            : [];

        return sorted.Select(p =>
        {
            decimal displayPrice = p.IsCombo
                ? (comboPriceMap.TryGetValue(p.NodeID, out var cp) ? cp : p.Price)
                : p.Price;
            decimal? displayPriceDiscount = p.IsCombo ? null : (p.PriceDiscount > 0 ? p.PriceDiscount : null);

            return new SearchSuggestionDTO
            {
                NodeAlias = p.NodeAlias,
                RelativeUrl = p.RelativeUrl,
                Fields = new SearchSuggestionFieldsDTO
                {
                    ProductName = p.ProductName,
                    Images = p.Images.Length > 0 && mediaMap.TryGetValue(p.Images[0], out var url) ? [url] : [],
                    Price = displayPrice,
                    PriceDiscount = displayPriceDiscount,
                    CategoryName = p.ParentNodeID.HasValue ? categoryMap.GetValueOrDefault(p.ParentNodeID.Value) : null
                }
            };
        });
    }

    public async Task<IPagedEnumerable<ProductByColorItemDTO>> GetProductsByColorAsync(int colorId, int page, int pageSize)
    {
        var now = DateTimeOffset.UtcNow;

        // Load only the minimal variant data needed for sorting + display
        var matchingVariants = await _lumStoreContext.ProductVariants
            .AsNoTracking()
            .Where(v => v.ShiprelayId > 0 && v.VariantName != null && v.ColorId == colorId)
            .Select(v => new { v.ItemID, v.ProductID, v.Stock, v.VariantName, v.SKU, ColorValue = v.Color != null ? v.Color.ColorValue : null })
            .ToListAsync();

        if (matchingVariants.Count == 0)
            return Enumerable.Empty<ProductByColorItemDTO>().AsPagedEnumerable(0);

        var bestVariantByProduct = matchingVariants
            .GroupBy(v => v.ProductID)
            .Select(g => g.OrderByDescending(v => v.Stock).ThenBy(v => v.ItemID).First())
            .ToDictionary(v => v.ProductID);

        var allProductIds = bestVariantByProduct.Keys.ToList();

        // Lightweight query: just the fields needed for sorting, filtered to published
        var sortData = await _lumStoreContext.Products
            .AsNoTracking()
            .Where(p =>
                allProductIds.Contains(p.NodeID) &&
                !p.IsDeleted &&
                (p.PublishedFrom == null || p.PublishedFrom <= now) &&
                (p.PublishedTo == null || p.PublishedTo > now))
            .Select(p => new { p.NodeID, p.IsBestSeller })
            .ToListAsync();

        if (sortData.Count == 0)
            return Enumerable.Empty<ProductByColorItemDTO>().AsPagedEnumerable(0);

        // Sort and paginate in memory on tiny dataset
        var sorted = sortData
            .Where(p => bestVariantByProduct.ContainsKey(p.NodeID))
            .OrderByDescending(p => p.IsBestSeller)
            .ThenByDescending(p => bestVariantByProduct[p.NodeID].Stock)
            .ThenBy(p => p.NodeID)
            .ToList();

        var totalRecords = sorted.Count;

        var pageNodeIds = sorted
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => p.NodeID)
            .ToList();

        // Load full product data for ONLY the current page
        var products = await _lumStoreContext.Products
            .AsNoTracking()
            .Where(p => pageNodeIds.Contains(p.NodeID))
            .Select(p => new
            {
                p.NodeID,
                p.Node.RelativeUrl,
                p.Node.NodeAlias,
                p.ProductName,
                p.Price,
                p.PriceDiscount,
                p.IsBestSeller,
                p.IsCombo,
                p.Images,
            })
            .ToListAsync();

        // Preserve sort order from the sorted list
        var pageItems = pageNodeIds
            .Where(id => products.Any(p => p.NodeID == id) && bestVariantByProduct.ContainsKey(id))
            .Select(id => (product: products.First(p => p.NodeID == id), variant: bestVariantByProduct[id]))
            .ToList();

        if (pageItems.Count == 0)
            return Enumerable.Empty<ProductByColorItemDTO>().AsPagedEnumerable(totalRecords);

        // Batch-compute combo prices — 2 DB queries total regardless of combo count
        var comboIds = pageItems.Where(x => x.product.IsCombo).Select(x => x.product.NodeID).Distinct().ToArray();
        var comboResults = comboIds.Length > 0
            ? await _discountRuleService.CalculateBatchComboPricesAsync(comboIds)
            : new Dictionary<int, ComboPriceResult>();
        var comboPriceMap = comboResults.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.TotalPrice);

        var imageGuids = pageItems
            .Where(x => x.product.Images.Length > 0)
            .Select(x => x.product.Images[0])
            .Distinct()
            .ToArray();

        var mediaMap = imageGuids.Length > 0
            ? (await _mediaService.GetMediaItemsAsync(imageGuids)).ToDictionary(m => m.FileID, m => m.FileURL)
            : new Dictionary<Guid, string>();

        var result2 = pageItems.Select(x =>
        {
            string? imageUrl = x.product.Images.Length > 0
                ? (mediaMap.TryGetValue(x.product.Images[0], out var url) ? url : null)
                : null;

            decimal displayPrice = x.product.IsCombo
                ? (comboPriceMap.TryGetValue(x.product.NodeID, out var cp) ? cp : x.product.Price)
                : x.product.Price;
            decimal displayPriceDiscount = x.product.IsCombo ? 0 : x.product.PriceDiscount;

            return new ProductByColorItemDTO
            {
                NodeID = x.product.NodeID,
                NodeAlias = x.product.NodeAlias,
                RelativeUrl = x.product.RelativeUrl,
                ProductName = x.product.ProductName,
                Price = displayPrice,
                PriceDiscount = displayPriceDiscount,
                Image = imageUrl,
                MatchedVariant = new MatchedVariantDTO
                {
                    VariantId = x.variant.ItemID,
                    VariantName = x.variant.VariantName,
                    Color = x.variant.ColorValue,
                    SKU = x.variant.SKU,
                    Stock = x.variant.Stock,
                }
            };
        });

        return result2.AsPagedEnumerable(totalRecords);
    }

    public async Task<ProductByColorDTO?> GetProductByColorAsync(int colorId)
    {
        var now = DateTimeOffset.UtcNow;

        var variantProductIds = await _lumStoreContext.ProductVariants
            .Where(v => v.ShiprelayId > 0 && v.VariantName != null && v.ColorId == colorId)
            .Select(v => v.ProductID)
            .Distinct()
            .ToListAsync();

        if (variantProductIds.Count == 0)
            return null;

        var product = await _lumStoreContext.Products
            .Where(p =>
                variantProductIds.Contains(p.NodeID) &&
                !p.IsDeleted &&
                (p.PublishedFrom == null || p.PublishedFrom <= now) &&
                (p.PublishedTo == null || p.PublishedTo > now))
            .OrderByDescending(p => p.IsBestSeller)
            .Select(p => new
            {
                p.NodeID,
                p.Node.NodeAlias,
                p.Node.RelativeUrl,
                p.ProductName,
                p.Images,
                ParentNodeID = p.Node.ParentNodeID,
            })
            .FirstOrDefaultAsync();

        if (product == null)
            return null;

        string? imageUrl = null;
        if (product.Images.Length > 0)
        {
            var media = await _mediaService.GetMediaItemsAsync([product.Images[0]]);
            imageUrl = media.FirstOrDefault()?.FileURL;
        }

        string? availableIn = null;
        if (product.ParentNodeID.HasValue)
        {
            availableIn = await _lumStoreContext.ProductCategories
                .Where(c => c.NodeID == product.ParentNodeID.Value)
                .Select(c => c.CategoryName)
                .FirstOrDefaultAsync();
        }

        return new ProductByColorDTO
        {
            RelativeUrl = product.RelativeUrl,
            NodeAlias = product.NodeAlias,
            ProductName = product.ProductName,
            AvailableIn = availableIn,
            Image = imageUrl,
        };
    }

    public async Task<IEnumerable<DocumentClientGetDTO>> GetRelatedProductsAsync(string relativeUrl, int limit)
    {
        limit = Math.Clamp(limit, 1, 15);
        var now = DateTimeOffset.UtcNow;

        var current = await _lumStoreContext.Products
            .Where(p => p.ProductVariants.Any(v => v.ShiprelayId > 0) && p.Node.RelativeUrl == relativeUrl && !p.IsDeleted)
            .Select(p => new { p.NodeID, ParentNodeID = p.Node.ParentNodeID })
            .FirstOrDefaultAsync();

        if (current == null || !current.ParentNodeID.HasValue)
            return [];

        var related = await _lumStoreContext.Products
            .Where(p =>
                p.Node.ParentNodeID == current.ParentNodeID.Value &&
                p.NodeID != current.NodeID &&
                p.ProductVariants.Any(v => v.ShiprelayId > 0) &&
                !p.IsDeleted &&
                (p.PublishedFrom == null || p.PublishedFrom <= now) &&
                (p.PublishedTo == null || p.PublishedTo > now))
            .OrderByDescending(p => p.IsBestSeller)
            .ThenBy(p => p.Node.NodeOrder)
            .Take(limit)
            .Select(p => new Product
            {
                NodeID = p.NodeID,
                PageID = p.PageID,
                ProductName = p.ProductName,
                Price = p.Price,
                PriceDiscount = p.PriceDiscount,
                IsBestSeller = p.IsBestSeller,
                IsCombo = p.IsCombo,
                Images = p.Images,
                ShortDescription = p.ShortDescription,
                PublishedFrom = p.PublishedFrom,
                PublishedTo = p.PublishedTo,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                Node = p.Node,
            })
            .ToListAsync();

        if (related.Count == 0) return [];

        var relatedNodeIds = related.Select(x => x.NodeID).ToArray();
        var imageGuids = related.SelectMany(x => x.Images).Distinct().ToArray();
        var images = imageGuids.Length > 0
            ? (await _mediaService.GetMediaItemsAsync(imageGuids)).ToList()
            : [];

        var variantsByProduct = await LoadVariantsAsync(related.Select(x => x.NodeID));
        var comboPrices = await GetComboPricesAsync(related);
        var discountTiers = await _discountRuleService.GetDiscountTiersForProductsAsync(relatedNodeIds);

        return related.Select(x =>
        {
            var productItem = new ProductClientDTO(x)
            {
                Images = images.Where(i => x.Images.Contains(i.FileID)).OrderBy(i => x.Images.IndexOf(i.FileID)).Select(i => i.FileURL).ToArray(),
                ProductVariants = variantsByProduct.GetValueOrDefault(x.NodeID, []),
                IsCombo = x.IsCombo,
                DiscountRules = discountTiers.GetValueOrDefault(x.NodeID, []),
            };

            if (x.IsCombo && comboPrices.TryGetValue(x.NodeID, out var cp))
            {
                productItem.Price = cp.TotalPrice;
                productItem.PriceDiscount = null;
                productItem.ComboStock = cp.ComboStock;
            }

            return new DocumentClientGetDTO(productItem, x);
        });
    }

    public async Task<IEnumerable<string>> GetSearchRecommendationsAsync(int limit)
    {
        limit = Math.Min(limit, 20);
        var sevenDaysAgo = DateTime.UtcNow.Date.AddDays(-7);
        var now = DateTimeOffset.UtcNow;

        return await _lumStoreContext.Products
            .Where(p =>
                !p.IsDeleted &&
                p.ProductVariants.Any(v => v.ShiprelayId > 0) &&
                (p.PublishedFrom == null || p.PublishedFrom <= now) &&
                (p.PublishedTo == null || p.PublishedTo > now) &&
                (p.IsBestSeller || p.CreatedAt >= sevenDaysAgo))
            .OrderBy(p => p.Node.NodeOrder)
            .Take(limit)
            .Select(p => p.ProductName)
            .ToListAsync();
    }

    /// <summary>
    /// For a list of products, computes combo price + stock for those with IsCombo = true.
    /// Returns a dictionary: NodeID -> ComboPriceResult.
    /// Uses a single batch call regardless of how many combo products are present.
    /// </summary>
    private async Task<Dictionary<int, ComboPriceResult>> GetComboPricesAsync(IEnumerable<Product> products)
    {
        var comboIds = products.Where(x => x.IsCombo).Select(x => x.NodeID).Distinct().ToArray();
        if (comboIds.Length == 0) return [];
        return await _discountRuleService.CalculateBatchComboPricesAsync(comboIds);
    }

    /// <summary>
    /// Batch-loads variants and resolves their image GUIDs to URLs in one media call.
    /// Returns a dictionary keyed by ProductID → list of CategoryProductVariantDTO.
    /// </summary>
    private async Task<Dictionary<int, List<CategoryProductVariantDTO>>> LoadVariantsRawAsync(IEnumerable<int> productIds)
    {
        var ids = productIds.ToList();
        if (ids.Count == 0) return [];

        var variants = await _productVariantRepository
        .GetProductVariants()
        .Include(x => x.Color)
        .AsNoTrackingWithIdentityResolution()
        .Where(v => v.ShiprelayId > 0 && ids.Contains(v.ProductID)).ToListAsync();

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
                    Color = v.Color?.ColorValue,
                    Stock = v.Stock,
                    SKU = v.SKU,
                    Images = variantImages
                        .Where(img => v.Images.Contains(img.FileID))
                        .OrderBy(img => v.Images.IndexOf(img.FileID))
                        .Select(img => img.FileURL)
                        .ToArray()
                }).ToList()
            );
    }

    /// <summary>
    /// Batch-loads variants for a set of product IDs and resolves their images in one media call.
    /// Returns a dictionary keyed by ProductID → list of mapped DTOs.
    /// </summary>
    private async Task<Dictionary<int, List<ProductVariantClientGetDTO>>> LoadVariantsAsync(IEnumerable<int> productIds)
    {
        var ids = productIds.ToList();
        if (ids.Count == 0)
            return [];

        var variants = (await _productVariantRepository.GetProductVariantsAsync(v => v.ShiprelayId > 0 && ids.Contains(v.ProductID))).ToList();
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
                g => g.Select(v => new ProductVariantClientGetDTO(
                    v,
                    variantImages.Where(img => v.Images.Contains(img.FileID)).OrderBy(img => v.Images.IndexOf(img.FileID)).ToArray()
                )).ToList()
            );
    }
}
