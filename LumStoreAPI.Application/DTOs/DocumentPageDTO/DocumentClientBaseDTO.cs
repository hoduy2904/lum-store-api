using System;
using System.Text.Json.Serialization;
using LumStoreAPI.Core.Entities.DocumentEngine;
using LumStoreAPI.Core.Interfaces.Sytems;
using LumStoreAPI.Core.Models.Systems;
using LumStoreAPI.Libraries.Helpers;

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
    public bool IsPublished { get; internal set; }

    [JsonIgnore]
    public IGenericFeatureQuery? FeatureQuery { get; set; }
    public WidgetData<object>[] DocumentPageWidgets { get; set; } = [];

    public DocumentClientBaseDTO(DocumentPage documentPage)
    {
        this.DocumentPageWidgets = documentPage.DocumentPageWidgets;
        if (DocumentPageTypeHelper.DocumentFeatureQueries.TryGetValue(documentPage.GetType(), out var query))
        {
            this.FeatureQuery = (IGenericFeatureQuery?)Activator.CreateInstance(query, documentPage);
        }
        this.NodeID = documentPage.NodeID;
        this.ClassName = documentPage.Node.ClassName;
        this.ParentNodeID = documentPage.Node?.ParentNodeID;
        this.DocumentName = documentPage.DocumentName;
        this.RelativeUrl = documentPage.Node?.RelativeUrl;
        this.NodeOrder = documentPage.Node?.NodeOrder ?? 0;
        this.NodeAlias = documentPage.Node?.NodeAlias ?? string.Empty;
        this.NodeName = documentPage.Node?.NodeName ?? string.Empty;
        this.IsPublished = documentPage.IsPublished;

    }
}
