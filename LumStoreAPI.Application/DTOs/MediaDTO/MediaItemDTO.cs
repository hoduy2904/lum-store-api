namespace LumStoreAPI.Application.DTOs.MediaDTO
{
    public class MediaItemDTO
    {
        public Guid FileID { get; set; }
        public string? Extension { get; set; }
        public string FileName { get; set; } = default!;
        public string? Title { get; set; }
        public int CategoryID { get; set; }
        public long FileSize { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public string FileURL { get; set; } = default!;
    }
}
