using LumStoreAPI.Application.DTOs.DocumentContents;

namespace LumStoreAPI.Application.DTOs.QueryDTOs
{
    public class HomePageFeatureDTO
    {
        public IEnumerable<CTAImageItemDTO> Carousels { get; set; } = [];
    }
}
