using System;
using System.Linq.Expressions;
using LumStoreAPI.Application.DTOs.DocumentPageDTO;
using LumStoreAPI.Application.DTOs.ProductDTO;
using LumStoreAPI.Application.DTOs.StoreDTO;
using LumStoreAPI.Application.Interfaces;
using LumStoreAPI.Core.Entities.Pages;
using LumStoreAPI.Core.Interfaces.ContentEngine;
using LumStoreAPI.Core.Interfaces.Repositories;
using LumStoreAPI.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Math.EC.Rfc7748;


namespace LumStoreAPI.Application.Services;

internal class ProductService
(
IPageRetrieveContext pageRetrieveContext,
LumStoreContext lumStoreContext,
IMediaService mediaService)
 : IProductService
{
    private readonly IPageRetrieveContext _pageRetrieveContext = pageRetrieveContext;
    private readonly IMediaService _mediaService = mediaService;
    private readonly LumStoreContext _lumStoreContext = lumStoreContext;
    public Task<IEnumerable<DocumentClientGetDTO>> GetFeatureProducts(int topN)
    {
        return this.GetProducts(x => x.IsBestSeller, topN);
    }

    public Task<IEnumerable<DocumentClientGetDTO>> GetNewProducts(int topN)
    {
        var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);
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
        var productDTOs = products.Select(x =>
        {
            var productItem = new ProductClientDTO(x)
            {
                Images = images.Where(i => x.Images.Contains(i.FileID)).Select(i => i.FileURL).ToArray()
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

        var imageGuids = products.SelectMany(x => x.Images).ToArray();
        var images = await _mediaService.GetMediaItemsAsync(imageGuids);
        var productDTOs = products.Select(x =>
        {
            var productItem = new ProductClientDTO(x)
            {
                Images = images.Where(i => x.Images.Contains(i.FileID)).Select(i => i.FileURL).ToArray()
            };

            var product = new DocumentClientGetDTO(productItem, x);
            return product;
        });

        return productDTOs;
    }
}
