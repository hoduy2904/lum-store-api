using System;
using LumStoreAPI.Core.Entities.DocumentEngine;

namespace LumStoreAPI.Application.DTOs.DocumentPageDTO;

public class DocumentClientBaseDTO
{
    public int NodeID { get; set; }
    public string NodeName { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public string DocumentName { get; set; } = default!;
    public int? ParentNodeID { get; set; }
    public string? RelativeUrl { get; set; }
    public int NodeOrder { get; set; }
    public string NodeAlias { get; set; } = default!;
    public object? SpecialContent { get; set; }
    public bool IsPublished { get; internal set; }

    public DocumentClientBaseDTO(DocumentPage documentPage)
    {
        this.NodeID = documentPage.NodeID;
        this.ClassName = documentPage.Node.ClassName;
        this.ParentNodeID = documentPage.Node?.ParentNodeID;
        this.DocumentName = documentPage.DocumentName;
        this.RelativeUrl = documentPage.Node?.RelativeUrl;
        this.NodeOrder = documentPage.Node?.NodeOrder ?? 0;
        this.NodeAlias = documentPage.Node?.NodeAlias ?? string.Empty;
        this.NodeName = documentPage.Node?.NodeName ?? string.Empty;
        this.IsPublished = (documentPage.PublishedFrom == null || documentPage.PublishedFrom <= DateTime.UtcNow) && (documentPage.PublishedTo == null || documentPage.PublishedTo > DateTime.UtcNow);

    }
}
