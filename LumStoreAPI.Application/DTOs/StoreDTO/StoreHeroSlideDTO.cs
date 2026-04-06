namespace LumStoreAPI.Application.DTOs.StoreDTO
{
    /// <summary>
    /// Hero slide DTO — mirrors lum-nails HeroSlide type exactly.
    /// Deserialized from HomePage.HeroSlidesJson.
    /// </summary>
    public class StoreHeroSlideDTO
    {
        public string Pretitle { get; set; } = default!;
        public string Title { get; set; } = default!;
        public string Description { get; set; } = default!;
        public string Image { get; set; } = default!;
        public string PrimaryCtaText { get; set; } = default!;
        public string PrimaryCtaLink { get; set; } = default!;
        public string SecondaryCtaText { get; set; } = default!;
        public string SecondaryCtaLink { get; set; } = default!;
    }
}
