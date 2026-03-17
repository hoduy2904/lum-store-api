using LumStoreAPI.Core.Entities.Base;

namespace LumStoreAPI.Core.Entities.Systems
{
    public class MediaLibrary : BaseItem
    {
        public Guid FileID { get; set; }
        public string FileName { get; set; } = default!;
        public string? Title { get; set; }
        public string? Extension { get; set; }
        public int CategoryID { get; set; }
        public long Size { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }

        public virtual MediaLibraryCategory MediaLibraryCategory { get; set; } = default!;
    }
}
