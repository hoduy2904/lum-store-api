using LumStoreAPI.Application.DTOs.DocumentContents;

namespace LumStoreAPI.Application.DTOs.QueryDTOs
{
    public class HomePageFeatureDTO
    {
        public string PageTitle { get; set; } = default!;
        public string? Description { get; set; }
        public IEnumerable<CTAImageItemDTO> Carousels { get; set; } = [];
    }
}
