using System;
using LumStoreAPI.Core.Entities.Systems;

namespace LumStoreAPI.Application.DTOs.MediaDTO;

public class MediaFolderDTO
{
    public int FolderID { get; set; }
    public string FolderName { get; set; } = default!;

    public MediaFolderDTO(MediaLibraryCategory category)
    {
        this.FolderID = category.CategoryID;
        this.FolderName = category.CategoryName;
    }
}
