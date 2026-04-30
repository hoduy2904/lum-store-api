using System;
using LumStoreAPI.Application.DTOs.Widgets;
using LumStoreAPI.Application.Interfaces;
using LumStoreAPI.Application.Widgets;
using MediatR;

namespace LumStoreAPI.Widgets;

public class ProductColorWidgetHandler
(
    IProductVariantService productVariantService
) : IRequestHandler<ProductColorWidget, ProductColorWidgetDTO>
{
    private readonly IProductVariantService _productVariantService = productVariantService;
    public async Task<ProductColorWidgetDTO> Handle(ProductColorWidget request, CancellationToken cancellationToken)
    {
        var colors = await _productVariantService.GetProductVariantColorsAsync(request.MaxColor);
        return new ProductColorWidgetDTO(colors);
    }
}
