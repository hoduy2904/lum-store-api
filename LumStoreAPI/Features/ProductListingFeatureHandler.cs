using LumStoreAPI.Application.DTOs.QueryDTOs;
using LumStoreAPI.Application.FeatureQueries;
using LumStoreAPI.Application.Interfaces;
using MediatR;

namespace LumStoreAPI.Features;

public class ProductListingFeatureHandler
(
    IProductService productService
)
 : IRequestHandler<ProductListingFeatureQuery, ProductListingFeatureDTO>
{
    private readonly IProductService _productService = productService;
    public async Task<ProductListingFeatureDTO> Handle(ProductListingFeatureQuery request, CancellationToken cancellationToken)
    {
        var model = new ProductListingFeatureDTO
        {
            Description = request.ProductCategory.CategoryDescription,
            Title = request.ProductCategory.CategoryName,
            Categories = await _productService.GetProductCategories(),
        };

        return model;
    }
}
