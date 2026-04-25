using LumStoreAPI.Application.DTOs.DocumentContents;
using LumStoreAPI.Application.DTOs.QueryDTOs;
using LumStoreAPI.Application.FeatureQueries;
using LumStoreAPI.Application.Interfaces;
using LumStoreAPI.Core.Entities.DocumentTypes;
using LumStoreAPI.Core.Interfaces.Repositories;
using MediatR;

namespace LumStoreAPI.Features
{
    public class HomeFeatureHandler(
        IPageRetrieveContext pageRetrieveContext,
        IMediaService mediaService) : IRequestHandler<HomePageFeatureQuery, HomePageFeatureDTO>
    {
        private readonly IPageRetrieveContext _pageRetrieveContext = pageRetrieveContext;
        private readonly IMediaService _mediaService = mediaService;
        public async Task<HomePageFeatureDTO> Handle(HomePageFeatureQuery request, CancellationToken cancellationToken)
        {
            var model = new HomePageFeatureDTO
            {
                Description = request.HomePage.Description,
                PageTitle = request.HomePage.PageTitle
            };

            var carouselItems = await _pageRetrieveContext.GetPagesAsync<CTAImageItem>(query =>
            {
                query.GetDescendants(request.HomePage.CarouselPathId);
            });

            var imageIds = carouselItems.Where(x => x.Image.Any()).SelectMany(x => x.Image!);
            var images = await _mediaService.GetMediaItemsAsync(imageIds.ToArray());
            model.Carousels = carouselItems.Select(x =>
            {
                var ctaImage = new CTAImageItemDTO(x)
                {
                    Image = images.FirstOrDefault(img => x.Image.Contains(img.FileID))?.FileURL
                };
                return ctaImage;
            });

            return model;

        }
    }
}
