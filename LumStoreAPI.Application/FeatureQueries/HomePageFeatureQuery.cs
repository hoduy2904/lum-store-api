using LumStoreAPI.Application.DTOs.QueryDTOs;
using LumStoreAPI.Core.Attributes;
using LumStoreAPI.Core.Entities.Pages;
using LumStoreAPI.Core.Interfaces.Sytems;
using MediatR;

namespace LumStoreAPI.Application.FeatureQueries
{
    [MappingFeatureQuery<HomePage, HomePageFeatureQuery>]
    public class HomePageFeatureQuery : IGenericFeatureQuery, IRequest<HomePageFeatureDTO>
    {
        public HomePage HomePage { get; set; }
        public HomePageFeatureQuery(HomePage homePage)
        {
            HomePage = homePage;
        }
    }
}
