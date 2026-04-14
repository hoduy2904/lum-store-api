using LumStoreAPI.Application.DTOs.MediaDTO;
using LumStoreAPI.Application.DTOs.ProductVariantDTO;
using LumStoreAPI.Application.DTOs.StoreDTO;
using LumStoreAPI.Application.Interfaces;
using LumStoreAPI.Core.Entities.Pages;
using LumStoreAPI.Core.Interfaces.ContentEngine;
using LumStoreAPI.Core.Interfaces.Repositories;
using LumStoreAPI.Core.Models.Enums;
using LumStoreAPI.Infrastructure;
using LumStoreAPI.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace LumStoreAPI.Application.Services
{
    internal class StoreService : IStoreService
    {
        private readonly IPageRetrieveContext _pageRetrieveContext;
        private readonly IProductVariantRepository _variantRepository;
        private readonly IMediaService _mediaService;
        private readonly LumStoreContext _context;

        public StoreService(
            IPageRetrieveContext pageRetrieveContext,
            IProductVariantRepository variantRepository,
            IMediaService mediaService,
            LumStoreContext context)
        {
            _pageRetrieveContext = pageRetrieveContext;
            _variantRepository = variantRepository;
            _mediaService = mediaService;
            _context = context;
        }

        // ─── Categories ───────────────────────────────────────────────────────

        public async Task<IEnumerable<StoreCategoryDTO>> GetCategoriesAsync()
        {
            var categories = await _pageRetrieveContext.GetPagesAsync<ProductCategory>(query =>
            {
                query
                    .Published(TreeNodePublished.All)
                    .IncludeQueryable(q => q.OrderBy(x => x.Node.NodeOrder));
            });

            // Count products per category in one batch query
            var categoryNodeIds = categories.Select(c => c.NodeID).ToList();

            var productCounts = await _context.DocumentLinkedNodes
                .Where(ln => categoryNodeIds.Contains(ln.Ancestor) && ln.Depth == 1)
                .Join(_context.DocumentPages,
                    ln => ln.Descendant,
                    p => p.NodeID,
                    (ln, p) => new { ln.Ancestor })
                .GroupBy(x => x.Ancestor)
                .Select(g => new { CategoryNodeId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.CategoryNodeId, x => x.Count);

            return categories.Select(c => new StoreCategoryDTO
            {
                Id = c.NodeID.ToString(),
                Slug = c.Node?.NodeAlias ?? "",
                Name = c.CategoryName,
                Image = c.CategoryImage,
                Description = c.CategoryDescription,
                ProductCount = productCounts.TryGetValue(c.NodeID, out var cnt) ? cnt : 0
            });
        }

        public async Task<StoreCategoryDTO?> GetCategoryAsync(string slug)
        {
            var categories = await _pageRetrieveContext.GetPagesAsync<ProductCategory>(query =>
            {
                query
                    .Published(TreeNodePublished.All)
                    .Where(x => x.Node.NodeAlias == slug);
            });

            var cat = categories.FirstOrDefault();
            if (cat == null) return null;
            return new StoreCategoryDTO
            {
                Id = cat.NodeID.ToString(),
                Slug = cat.Node?.NodeAlias ?? "",
                Name = cat.CategoryName,
                Image = cat.CategoryImage,
                Description = cat.CategoryDescription,
                ProductCount = 0
            };
        }

        // ─── Products ─────────────────────────────────────────────────────────

        public async Task<IPagedEnumerable<StoreProductDTO>> GetProductsAsync(StoreProductListRequest request)
        {
            // Resolve category NodeID if category filter supplied
            int? categoryNodeId = null;
            if (!string.IsNullOrEmpty(request.Category))
            {
                categoryNodeId = await _context.DocumentNodes
                    .Where(n => n.NodeAlias == request.Category)
                    .Select(n => (int?)n.NodeID)
                    .FirstOrDefaultAsync();
            }

            // Fetch paged products using PageRetrieveContext
            var pagedProducts = await _pageRetrieveContext.GetPagedPagesAsync<Product>(query =>
            {
                query.Published(TreeNodePublished.All);

                // Category filter: direct children of category node
                if (categoryNodeId.HasValue)
                    query.GetDescendants(categoryNodeId.Value, 1);

                // Field filters
                query.Where(x =>
                    (string.IsNullOrEmpty(request.Search) ||
                        x.ProductName.Contains(request.Search) ||
                        x.ShortDescription!.Contains(request.Search))
                    && (!request.IsNew.HasValue || x.IsNew == request.IsNew)
                    && (!request.IsBestSeller.HasValue || x.IsBestSeller == request.IsBestSeller)
                    && (!request.IsSale.HasValue || (request.IsSale.Value ? x.PriceDiscount > 0 : x.PriceDiscount == 0))
                    && (!request.MinPrice.HasValue || (x.Price - x.PriceDiscount) >= request.MinPrice)
                    && (!request.MaxPrice.HasValue || (x.Price - x.PriceDiscount) <= request.MaxPrice)
                );

                // Sorting
                query.IncludeQueryable(q => request.SortBy switch
                {
                    "price_asc" => q.OrderBy(x => x.Price - x.PriceDiscount),
                    "price_desc" => q.OrderByDescending(x => x.Price - x.PriceDiscount),
                    "newest" => q.OrderByDescending(x => x.IsNew).ThenByDescending(x => x.PageID),
                    "bestseller" => q.OrderByDescending(x => x.IsBestSeller).ThenByDescending(x => x.ReviewCount),
                    "rating" => q.OrderByDescending(x => x.Rating),
                    _ => q.OrderBy(x => x.Node.NodeOrder)
                });

                query.Paged(request.Page, request.PageSize);
            });

            int total = pagedProducts.TotalRecords;
            var products = pagedProducts.ToList();

            var dtos = await MapProductsAsync(products);
            return dtos.AsPagedEnumerable(total);
        }

        public async Task<IEnumerable<StoreProductDTO>> GetFeaturedProductsAsync(int limit = 8)
        {
            var products = await _pageRetrieveContext.GetPagesAsync<Product>(query =>
            {
                query
                    .Published(TreeNodePublished.All)
                    .Where(x => x.IsBestSeller || x.IsNew)
                    .IncludeQueryable(q => q
                        .OrderByDescending(x => x.IsBestSeller)
                        .ThenByDescending(x => x.ReviewCount)
                        .Take(limit));
            });

            return await MapProductsAsync(products.ToList());
        }

        public async Task<IEnumerable<StoreProductDTO>> GetNewArrivalsAsync(int limit = 4)
        {
            var products = await _pageRetrieveContext.GetPagesAsync<Product>(query =>
            {
                query
                    .Published(TreeNodePublished.All)
                    .Where(x => x.IsNew)
                    .IncludeQueryable(q => q.OrderByDescending(x => x.PageID).Take(limit));
            });

            return await MapProductsAsync(products.ToList());
        }

        public async Task<StoreProductDTO?> GetProductAsync(string slug)
        {
            var products = await _pageRetrieveContext.GetPagesAsync<Product>(query =>
            {
                query
                    .Published(TreeNodePublished.All)
                    .Where(x => x.Node.NodeAlias == slug);
            });

            var product = products.FirstOrDefault();
            if (product == null) return null;

            var dtos = await MapProductsAsync([product]);
            return dtos.FirstOrDefault();
        }

        public async Task<IEnumerable<ProductVariantGetDTO>> GetProductVariantsAsync(string slug)
        {
            // Get PageID for this product
            var pageId = await _context.DocumentNodes
                .Where(n => n.NodeAlias == slug)
                .Join(_context.DocumentPages, n => n.NodeID, p => p.NodeID, (n, p) => (int?)p.PageID)
                .FirstOrDefaultAsync();

            if (pageId == null) return [];

            var variants = await _variantRepository.GetProductVariantsAsync(v => v.ProductID == pageId);
            if (!variants.Any()) return [];

            // Resolve media for variants that have images
            var allImageIds = variants.SelectMany(v => v.Images).Distinct().ToArray();
            var mediaItems = allImageIds.Length > 0
                ? (await _mediaService.GetMediaItemsAsync(allImageIds)).ToList()
                : [];

            return variants.Select(v =>
            {
                var variantMedia = mediaItems.Where(m => v.Images.Contains(m.FileID)).ToArray();
                return new ProductVariantGetDTO(v, variantMedia);
            });
        }

        // ─── Hero Slides ──────────────────────────────────────────────────────

        public async Task<IEnumerable<StoreHeroSlideDTO>> GetHeroSlidesAsync()
        {
            var homePages = await _pageRetrieveContext.GetPagesAsync<HomePage>(query =>
            {
                query
                    .Published(TreeNodePublished.All);
            });

            var homePage = homePages.FirstOrDefault();

            try
            {
                var slides = JsonSerializer.Deserialize<StoreHeroSlideDTO[]>(
                    homePage.Description,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return slides ?? [];
            }
            catch
            {
                return [];
            }
        }

        // ─── Private helpers ──────────────────────────────────────────────────

        /// <summary>
        /// Maps a batch of Product entities to StoreProductDTO.
        /// Fetches categories and variants in two batch queries (no N+1).
        /// </summary>
        private async Task<List<StoreProductDTO>> MapProductsAsync(List<Product> products)
        {
            if (!products.Any()) return [];

            // Batch 1: category info for all parent nodes
            var parentNodeIds = products
                .Select(p => p.Node?.ParentNodeID)
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .Distinct()
                .ToList();

            var categoryMap = new Dictionary<int, (string Name, string Slug)>();
            if (parentNodeIds.Count > 0)
            {
                var cats = await _pageRetrieveContext.GetPagesAsync<ProductCategory>(query =>
                {
                    query
                        .Published(TreeNodePublished.All)
                        .Where(x => parentNodeIds.Contains(x.NodeID));
                });

                foreach (var cat in cats)
                    categoryMap[cat.NodeID] = (cat.CategoryName, cat.Node?.NodeAlias ?? "");
            }

            // Batch 2: all variants for these products
            var productIds = products.Select(p => p.PageID).ToList();
            var allVariants = await _variantRepository.GetProductVariantsAsync(
                v => productIds.Contains(v.ProductID));

            var variantsByProductId = allVariants
                .GroupBy(v => v.ProductID)
                .ToDictionary(g => g.Key, g => g.ToList());

            // Batch 3: media images (only for products that have images)
            var allImageIds = products
                .Where(p => p.Images.Length > 0)
                .SelectMany(p => p.Images)
                .Distinct()
                .ToArray();

            var mediaItems = allImageIds.Length > 0
                ? (await _mediaService.GetMediaItemsAsync(allImageIds)).ToList()
                : new List<MediaItemDTO>();

            // Map each product
            return products.Select(p =>
            {
                var parentId = p.Node?.ParentNodeID;
                var (catName, catSlug) = parentId.HasValue && categoryMap.TryGetValue(parentId.Value, out var cv)
                    ? cv
                    : ("", "");

                var variants = variantsByProductId.TryGetValue(p.PageID, out var vl) ? vl : [];
                var firstSku = variants.FirstOrDefault()?.SKU ?? "";
                var totalStock = variants.Sum(v => v.Stock);

                // Resolve product-level images
                var productImages = p.Images.Length > 0
                    ? mediaItems.Where(m => p.Images.Contains(m.FileID)).Select(m => m.FileURL).ToArray()
                    : [];

                var salePrice = p.Price - p.PriceDiscount;

                return new StoreProductDTO
                {
                    Id = p.PageID.ToString(),
                    Slug = p.Node?.NodeAlias ?? "",
                    Title = p.ProductName,
                    Sku = firstSku,
                    Price = salePrice,
                    OldPrice = p.PriceDiscount > 0 ? p.Price : null,
                    Images = productImages,
                    Category = catName,
                    CategorySlug = catSlug,
                    Tags = p.Tags,
                    Description = p.Description,
                    ShortDescription = p.ShortDescription,
                    IsNew = p.IsNew,
                    IsBestSeller = p.IsBestSeller,
                    IsSale = p.PriceDiscount > 0,
                    Rating = p.Rating,
                    ReviewCount = p.ReviewCount,
                    Stock = totalStock
                };
            }).ToList();
        }
    }
}
