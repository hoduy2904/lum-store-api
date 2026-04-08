using LumStoreAPI.Application.DTOs.Widgets;
using LumStoreAPI.Core.Attributes;
using LumStoreAPI.Core.Models.Controls;
using MediatR;

namespace LumStoreAPI.Application.Widgets
{
    [RegisterWidget("ctaFullWidthImage", typeof(CTAFullWidthImageWidget))]
    public class CTAFullWidthImageWidget : IRequest<CTAFullWidthImageDTO>
    {
        public Guid[]? CTAImage { get; set; }
        public string? Pretitle { get; set; }
        public string? CTAHeader { get; set; }
        public string? CTADescription { get; set; }
        public LinkControl? CTALink { get; set; }
    }
}
