using LumStoreAPI.Core.Models.Controls;

namespace LumStoreAPI.Application.DTOs.Widgets
{
    public class CTAFullWidthImageDTO
    {
        public string? Pretitle { get; set; }
        public string? CTAHeader { get; set; }
        public string? CTADescription { get; set; }
        public string[] CTAImages { get; set; } = [];
        public LinkControl? CTALink { get; set; }
    }
}
