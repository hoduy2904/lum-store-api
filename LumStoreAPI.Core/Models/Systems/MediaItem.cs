using LumStoreAPI.Core.Entities.Systems;

namespace LumStoreAPI.Core.Models.Systems
{
    public class MediaItem
    {
        public Guid FileID { get; set; }
        public string FileName { get; set; } = default!;
        public int CategoryID { get; set; }
        public string? CategoryPath { get; set; }
        public string? Extension { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public long Size { get; set; }
        public string? Title { get; set; }

        public MediaItem()
        {
            
        }

        public MediaItem(MediaLibrary mediaLibrary)
        {
            this.FileID = mediaLibrary.FileID;
            this.FileName = mediaLibrary.FileName;
            this.CategoryID = mediaLibrary.CategoryID;
            this.Extension = mediaLibrary.Extension;
            this.Size = mediaLibrary.Size;
            this.CategoryPath = mediaLibrary.MediaLibraryCategory?.FolderName;
        }
    }
}
