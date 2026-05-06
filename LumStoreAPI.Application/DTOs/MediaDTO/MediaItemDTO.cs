using System.Text.Json.Serialization;
using LumStoreAPI.Core.Entities.Systems;
using LumStoreAPI.Libraries.Helpers;

namespace LumStoreAPI.Application.DTOs.MediaDTO;

public class MediaItemDTO
{
    public Guid FileID { get; set; }
    public string FileName { get; set; } = default!;
    public int CategoryID { get; set; }
    [JsonIgnore]
    public string? CategoryPath { get; set; }
    [JsonIgnore]
    public string FullDirectPath { get; internal set; }
    public string? Extension { get; set; }
    public int Height { get; set; }
    public int Width { get; set; }
    public long Size { get; set; }
    public string? Title { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public string FileURL { get; internal set; }
    public string RelativeURL { get; internal set; }

    public MediaItemDTO(MediaLibrary mediaLibrary)
    {
        this.FileID = mediaLibrary.FileID;
        this.FileName = mediaLibrary.FileName;
        this.CategoryID = mediaLibrary.CategoryID;
        this.CategoryPath = mediaLibrary.MediaLibraryCategory.FolderName;
        this.Extension = mediaLibrary.Extension;
        this.Height = mediaLibrary.Height;
        this.Width = mediaLibrary.Width;
        this.Size = mediaLibrary.Size;
        this.Title = mediaLibrary.Title;
        this.CreatedAt = mediaLibrary.CreatedAt;
        this.UpdatedAt = mediaLibrary.UpdatedAt;
        this.FullDirectPath = MediaLibraryHelper.GetDirectMediaFilePath(mediaLibrary);
        this.FileURL = MediaLibraryHelper.GetAbsoluteFileURL(mediaLibrary);
        this.RelativeURL = MediaLibraryHelper.GetFileURL(mediaLibrary);
    }
}
