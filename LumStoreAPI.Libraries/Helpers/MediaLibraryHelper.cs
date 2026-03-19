using LumStoreAPI.Core.Entities.Systems;
using LumStoreAPI.Core.Models.Systems;

namespace LumStoreAPI.Libraries.Helpers
{
    public class MediaLibraryHelper
    {
        public static string RootMediaPath { get; set; } = string.Empty;

        public static string GetDirectPath(string path) => Path.Combine(RootMediaPath, path).ToLowerInvariant();
        public static string GetDirectMediaFilePath(MediaLibrary mediaLibrary)
        {
            return GetDirectPath(Path.Combine(mediaLibrary.MediaLibraryCategory.FolderName, $"{mediaLibrary.FileID}{mediaLibrary.Extension}"));
        }

        public static Task<byte[]> GetMediaLibraryBytesAsync(MediaLibrary mediaLibrary)
        {
            var directPath = GetDirectMediaFilePath(mediaLibrary);
            return File.ReadAllBytesAsync(directPath);
        }

        public static Stream GetMediaLibraryStream(MediaLibrary mediaLibrary)
        {
            var directPath = GetDirectMediaFilePath(mediaLibrary);
            return GetFileStream(directPath);
        }
        public static Stream GetFileStream(string path)
        {
            return new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        }
        public static byte[] GetMediaLibraryBytes(MediaLibrary mediaLibrary)
        {
            var directPath = GetDirectMediaFilePath(mediaLibrary); return File.ReadAllBytes(directPath);
        }

        public static string GetFileURL(MediaLibrary mediaLibrary)
        {
            return $"/api/media/getFile?fileId={mediaLibrary.FileID}&format={mediaLibrary.Extension}";
        }

    }
}
