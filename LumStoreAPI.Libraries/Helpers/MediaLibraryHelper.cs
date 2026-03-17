using LumStoreAPI.Core.Models.Systems;

namespace LumStoreAPI.Libraries.Helpers
{
    public class MediaLibraryHelper
    {
        public static string RootMediaPath { get; set; } = string.Empty;

        public static string GetDirectPath(string path) => Path.Combine(RootMediaPath, path);

        public static Task<byte[]> GetMediaLibraryBytesAsync(MediaItem mediaItem)
        {
            var directPath = Path.Combine(RootMediaPath, mediaItem.CategoryPath ?? "", $"{mediaItem.FileID.ToString()}{mediaItem.Extension}");
            return File.ReadAllBytesAsync(directPath);
        }

        public static Stream GetMediaLibraryStream(MediaItem mediaItem)
        {
            var directPath = Path.Combine(RootMediaPath, mediaItem.CategoryPath ?? "", $"{mediaItem.FileID.ToString()}{mediaItem.Extension}");
            return new FileStream(directPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        }
        public static byte[] GetMediaLibraryBytes(MediaItem mediaItem)
        {
            var directPath = Path.Combine(RootMediaPath, mediaItem.CategoryPath ?? "", $"{mediaItem.FileID.ToString()}{mediaItem.Extension}");
            return File.ReadAllBytes(directPath);
        }

        public static string GetFileURL(MediaItem mediaItem)
        {
            return $"/api/media/getFile?fileId={mediaItem.FileID}&format={mediaItem.Extension}";
        }
    }
}
