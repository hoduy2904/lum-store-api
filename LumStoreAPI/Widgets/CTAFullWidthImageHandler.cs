using LumStoreAPI.Application.DTOs.Widgets;
using LumStoreAPI.Application.Interfaces;
using LumStoreAPI.Application.Widgets;
using LumStoreAPI.Core.Models.Controls;
using LumStoreAPI.Libraries.Helpers;
using MediatR;

namespace LumStoreAPI.Widgets
{
    public class CTAFullWidthImageHandler(
        IMediaService mediaService
        ) : IRequestHandler<CTAFullWidthImageWidget, CTAFullWidthImageDTO>
    {
        private readonly IMediaService _mediaService = mediaService;
        public async Task<CTAFullWidthImageDTO> Handle(CTAFullWidthImageWidget request, CancellationToken cancellationToken)
        {
            var ctaFullWidthImages = new CTAFullWidthImageDTO
            {
                CTADescription = request.CTADescription,
                CTAHeader = request.CTAHeader,
                Pretitle = request.Pretitle,
                CTALink = request.CTALink
            };
            if (request.CTAImage is not null && request.CTAImage.Length > 0)
            {
                ctaFullWidthImages.CTAImages = (await _mediaService.GetMediaItemsAsync(request.CTAImage)).Select(x => x.FileURL).ToArray();
            }

            return ctaFullWidthImages;
        }
    }
}
