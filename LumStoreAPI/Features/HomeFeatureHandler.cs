using LumStoreAPI.Application.DTOs.QueryDTOs;
using LumStoreAPI.Application.FeatureQueries;
using MediatR;

namespace LumStoreAPI.Features
{
    public class HomeFeatureHandler : IRequestHandler<HomePageFeatureQuery, HomePageFeatureDTO>
    {
        public Task<HomePageFeatureDTO> Handle(HomePageFeatureQuery request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
