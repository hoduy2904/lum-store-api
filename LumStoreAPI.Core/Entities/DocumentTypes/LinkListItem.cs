using LumStoreAPI.Core.Attributes;
using LumStoreAPI.Core.Entities.DocumentEngine;

namespace LumStoreAPI.Core.Entities.DocumentTypes;

public class LinkListItem : DocumentPage
{
    public const string CLASS_NAME = "Item.LinkList";

    [DocumentName]
    public string LinkListTitle { get; set; } = default!;
    public Guid[] LinkListIcon { get; set; } = [];
    public string? LinkUrl { get; set; }
}
