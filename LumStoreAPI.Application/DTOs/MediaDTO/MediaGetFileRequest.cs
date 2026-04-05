namespace LumStoreAPI.Application.DTOs.MediaDTO
{
    public class MediaGetFileRequest
    {
        public string? Format { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }
}
