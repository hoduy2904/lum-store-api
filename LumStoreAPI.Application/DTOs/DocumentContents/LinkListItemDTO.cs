using LumStoreAPI.Core.Entities.DocumentTypes;
using LumStoreAPI.Core.Models.Controls;

namespace LumStoreAPI.Application.DTOs.DocumentContents;

public record class LinkListItemDTO
{
    public string Title { get; set; }
    public string? Icon { get; set; }
    public LinkControl? Link { get; set; }

    public LinkListItemDTO(LinkListItem linkListItem, string? icon = null)
    {
        this.Title = linkListItem.LinkListTitle;
        this.Icon = icon;
        this.Link = !string.IsNullOrEmpty(linkListItem.LinkUrl)
            ? new LinkControl { Name = linkListItem.LinkListTitle, Url = linkListItem.LinkUrl, Target = null }
            : null;
    }
}
