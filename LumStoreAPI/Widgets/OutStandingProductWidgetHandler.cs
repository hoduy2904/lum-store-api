using LumStoreAPI.Application.DTOs.Widgets;
using LumStoreAPI.Application.Interfaces;
using LumStoreAPI.Application.Widgets;
using MediatR;

namespace LumStoreAPI.Widgets;

public class OutStandingProductWidgetHandler
(
    IMediaService mediaService,
    IProductService productService
) : IRequestHandler<OutStandingProductWidget, OutStandingProductWidgetDTO>
{
    private readonly IMediaService _mediaService = mediaService;
    private readonly IProductService _productService = productService;
    public async Task<OutStandingProductWidgetDTO> Handle(OutStandingProductWidget request, CancellationToken cancellationToken)
    {
        var model = new OutStandingProductWidgetDTO()
        {
            Pretitle = request.Pretitle,
            CTADescription = request.CTADescription,
            CTAHeader = request.CTAHeader,
            CTALink = request.CTALink,
            Type = request.Type,
        };
        if (request.CTAImage is not null && request.CTAImage.Length > 0)
        {
            model.CTAImages = (await _mediaService.GetMediaItemsAsync(request.CTAImage)).Select(x => x.FileURL).ToArray();
        }

        if (new[] { "feature", "new" }.Contains(request.Type.ToLower()))
        {
            if (request.Type == "feature")
            {
                model.Products = await _productService.GetFeatureProducts(4);
            }
            else
            {
                model.Products = await _productService.GetNewProducts(3);
            }
        }

        return model;
    }
}
