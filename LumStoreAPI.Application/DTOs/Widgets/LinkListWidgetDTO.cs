using System;
using LumStoreAPI.Application.DTOs.DocumentContents;

namespace LumStoreAPI.Application.DTOs.Widgets;

public class LinkListWidgetDTO
{
    public IEnumerable<LinkListItemDTO> Items { get; set; } = [];
}
