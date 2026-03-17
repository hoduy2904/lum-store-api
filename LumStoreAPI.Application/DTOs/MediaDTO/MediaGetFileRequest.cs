namespace LumStoreAPI.Application.DTOs.MediaDTO
{
    public class MediaGetFileRequest
    {
        public Guid FileID { get; set; }
        public string? Format { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }
}
