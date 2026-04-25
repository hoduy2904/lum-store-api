using LumStoreAPI.Core.Attributes;
using LumStoreAPI.Core.Entities.Pages;
using LumStoreAPI.Core.Interfaces.Sytems;
using LumStoreAPI.Application.DTOs.ProductDTO;
using MediatR;

namespace LumStoreAPI.Application.FeatureQueries;

[MappingFeatureQuery<Product, ProductFeatureQuery>]
public class ProductFeatureQuery : IGenericFeatureQuery, IRequest<ProductClientDTO>
{
    public Product Product { get; set; }

    public ProductFeatureQuery(Product product)
    {
        Product = product;
    }
}
