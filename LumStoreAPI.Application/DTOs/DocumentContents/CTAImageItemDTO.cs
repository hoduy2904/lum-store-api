using LumStoreAPI.Core.Entities.DocumentTypes;
using LumStoreAPI.Core.Models.Controls;

namespace LumStoreAPI.Application.DTOs.DocumentContents
{
    public record class CTAImageItemDTO
    {
        public string? Pretitle { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Type { get; set; }
        public string? Image { get; set; }
        public LinkControl? PrimaryButton { get; set; }

        public CTAImageItemDTO(CTAImageItem ctaImageItem)
        {
            this.Pretitle = ctaImageItem.Pretitle;
            this.Title = ctaImageItem.Title;
            this.Type = ctaImageItem.Type;
            this.Description = ctaImageItem.Description;
            this.PrimaryButton = ctaImageItem.PrimaryButton;
        }
    }
}
