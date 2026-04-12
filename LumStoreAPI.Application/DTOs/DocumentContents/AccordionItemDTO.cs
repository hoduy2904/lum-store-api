using System;
using LumStoreAPI.Core.Entities.DocumentTypes;

namespace LumStoreAPI.Application.DTOs.DocumentContents;

public class AccordionItemDTO
{
    public string Title { get; set; }
    public string? Description { get; set; }

    public AccordionItemDTO(AccordionItem accordionItem)
    {
        this.Title = accordionItem.Title;
        this.Description = accordionItem.Description;
    }
}
