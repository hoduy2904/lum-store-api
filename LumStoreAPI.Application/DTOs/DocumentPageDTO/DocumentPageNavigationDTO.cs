namespace LumStoreAPI.Application.DTOs.DocumentPageDTO
{
    public class DocumentPageNavigationDTO
    {
        public bool IsEnable { get; set; }
        public string? OgTitle { get; set; }
        public string? OgDescription { get; set; }
        public Guid[] OgImage { get; set; } = [];
    }
}
