using LumStoreAPI.Core.Entities.Systems;
using LumStoreAPI.Core.Models.Systems;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;

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
            string? extension = mediaLibrary.Extension;
            string[] _permittedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
            if (_permittedExtensions.Contains(mediaLibrary.Extension, StringComparer.OrdinalIgnoreCase))
            {
                extension = ".webp";
            }
            return $"/api/media/getFile/{mediaLibrary.FileID}?format={extension}";
        }
        public static string GetAbsoluteFileURL(MediaLibrary mediaLibrary)
        {
            try
            {
                return new Uri(new Uri(AppConfiguration.AdminConfiguration.AdminDomain), GetFileURL(mediaLibrary)).AbsoluteUri;
            }
            catch
            {
                return GetFileURL(mediaLibrary);
            }
        }

        public static string GetAbsoluteFileURL(MediaLibrary mediaLibrary, HttpContext httpContext)
        {
            return UriHelper.BuildAbsolute(httpContext.Request.Scheme, httpContext.Request.Host, GetFileURL(mediaLibrary));
        }

    }
}
