using System;
using LumStoreAPI.Application.DTOs.DocumentContents;

namespace LumStoreAPI.Application.DTOs.Widgets;

public class AccordionWidgetDTO
{
    public IEnumerable<AccordionItemDTO> Items { get; set; } = [];
}
