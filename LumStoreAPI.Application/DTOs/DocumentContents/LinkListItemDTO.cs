using System;
using LumStoreAPI.Core.Entities.DocumentTypes;

namespace LumStoreAPI.Application.DTOs.DocumentContents;

public record class LinkListItemDTO
{
    public string Title { get; set; }
    public string? Icon { get; set; }
    public string? Link { get; set; }

    public LinkListItemDTO(LinkListItem linkListItem, string? image = null)
    {
        this.Title = linkListItem.LinkListTitle;
        this.Icon = image;
        this.Link = linkListItem.LinkUrl;
    }
}
