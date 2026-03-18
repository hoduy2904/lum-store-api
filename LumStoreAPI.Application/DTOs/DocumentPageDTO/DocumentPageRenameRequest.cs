using System;
using System.ComponentModel.DataAnnotations;

namespace LumStoreAPI.Application.DTOs.DocumentPageDTO;

public class DocumentPageRenameRequest
{
    [Range(1, int.MaxValue)]
    public int NodeID { get; set; }
    [MinLength(1)]
    public string DocumentName { get; set; } = default!;
}
