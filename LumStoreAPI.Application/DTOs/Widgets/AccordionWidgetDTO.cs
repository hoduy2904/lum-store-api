using System;
using LumStoreAPI.Application.DTOs.DocumentContents;

namespace LumStoreAPI.Application.DTOs.Widgets;

public record class AccordionWidgetDTO(
    string? Title,
    string? Description,
    IEnumerable<AccordionItemDTO> Items = default!
)
{
    public IEnumerable<AccordionItemDTO> Items { get; set; } = Items ?? [];
}
