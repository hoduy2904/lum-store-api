using LumStoreAPI.Core.Models.Systems;

namespace LumStoreAPI.Libraries.Helpers
{
    public class MediaLibraryHelper
    {
        public static string RootMediaPath { get; set; } = string.Empty;

        public static Task<byte[]> GetMediaLibraryBytesAsync(MediaItem mediaItem)
        {
            var directPath = Path.Combine(RootMediaPath, mediaItem.CategoryPath ?? "", mediaItem.FileID.ToString());
            return File.ReadAllBytesAsync(directPath);
        }
        public static byte[] GetMediaLibraryBytes(MediaItem mediaItem)
        {
            var directPath = Path.Combine(RootMediaPath, mediaItem.CategoryPath ?? "", mediaItem.FileID.ToString());
            return File.ReadAllBytes(directPath);
        }
    }
}
