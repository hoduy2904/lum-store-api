using System;
using LumStoreAPI.Application.DTOs.QueryDTOs;
using LumStoreAPI.Core.Attributes;
using LumStoreAPI.Core.Entities.Pages;
using LumStoreAPI.Core.Interfaces.Sytems;
using MediatR;

namespace LumStoreAPI.Application.FeatureQueries;

[MappingFeatureQuery<ProductCategory, ProductListingFeatureQuery>]
public class ProductListingFeatureQuery : IGenericFeatureQuery, IRequest<ProductListingFeatureDTO>
{
    public ProductCategory ProductCategory { get; set; }
    public ProductListingFeatureQuery(ProductCategory productCategory)
    {
        this.ProductCategory = productCategory;
    }
}
