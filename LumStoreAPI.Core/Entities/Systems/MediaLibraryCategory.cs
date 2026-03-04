namespace LumStoreAPI.Core.Entities.Systems
{
    public class MediaLibraryCategory
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; } = default!;
        public string FolderName { get; set; } = default!;
        public virtual ICollection<MediaLibrary> MediaLibraries { get; set; } = [];
    }
}
