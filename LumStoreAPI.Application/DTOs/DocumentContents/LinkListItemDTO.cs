using LumStoreAPI.Core.Entities.DocumentTypes;
using LumStoreAPI.Core.Models.Controls;

namespace LumStoreAPI.Application.DTOs.DocumentContents;

public record class LinkListItemDTO
{
    public string Title { get; set; }
    public string? IconName { get; set; }
    public LinkControl? Link { get; set; }

    public LinkListItemDTO(LinkListItem linkListItem)
    {
        this.Title = linkListItem.LinkListTitle;
        this.IconName = linkListItem.IconName;
        this.Link = !string.IsNullOrEmpty(linkListItem.LinkUrl)
            ? new LinkControl { Name = linkListItem.LinkListTitle, Url = linkListItem.LinkUrl, Target = null }
            : null;
    }
}
