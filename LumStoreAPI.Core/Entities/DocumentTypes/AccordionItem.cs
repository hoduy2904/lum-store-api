using LumStoreAPI.Core.Attributes;
using LumStoreAPI.Core.Entities.DocumentEngine;

namespace LumStoreAPI.Core.Entities.DocumentTypes;

public class AccordionItem : DocumentPage
{
    public const string CLASS_NAME = "Item.Accordion";
    [DocumentName]
    public string Title { get; set; } = default!;
    public string? Description { get; set; }
}
