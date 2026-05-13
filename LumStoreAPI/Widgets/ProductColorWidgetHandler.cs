using System;
using LumStoreAPI.Application.DTOs.Widgets;
using LumStoreAPI.Application.Interfaces;
using LumStoreAPI.Application.Widgets;
using MediatR;

namespace LumStoreAPI.Widgets;

public class ProductColorWidgetHandler
(
    IColorService colorService
) : IRequestHandler<ProductColorWidget, ProductColorWidgetDTO>
{
    private readonly IColorService _colorService = colorService;
    public async Task<ProductColorWidgetDTO> Handle(ProductColorWidget request, CancellationToken cancellationToken)
    {
        var colors = await _colorService.GetGroupColorsAsync();
        return new ProductColorWidgetDTO(request.Title, request.Description, colors);
    }
}
