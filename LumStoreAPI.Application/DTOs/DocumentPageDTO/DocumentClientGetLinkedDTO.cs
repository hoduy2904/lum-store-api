using System;
using LumStoreAPI.Core.Entities.DocumentEngine;

namespace LumStoreAPI.Application.DTOs.DocumentPageDTO;

public class DocumentClientGetLinkedDTO : DocumentClientBaseDTO
{
    public DocumentClientGetLinkedDTO(DocumentPage documentPage) : base(documentPage)
    {
    }

    public IEnumerable<DocumentClientGetLinkedDTO> Children { get; set; } = [];
}
