using System;
using System.ComponentModel.DataAnnotations;

namespace LumStoreAPI.Application.DTOs.MediaDTO;

public class MediaFolderRequest
{
    [MinLength(2)]
    public string FolderName { get; set; } = default!;
}
