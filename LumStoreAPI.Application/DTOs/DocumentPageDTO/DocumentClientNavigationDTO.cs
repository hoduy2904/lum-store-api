namespace LumStoreAPI.Application.DTOs.DocumentPageDTO
{
    public class DocumentClientNavigationDTO
    {
        public string? OgTitle { get; set; }
        public string? OgDescription { get; set; }
        public string[] OgImage { get; set; } = [];
        public bool IsEnable { get; set; }
    }
}
