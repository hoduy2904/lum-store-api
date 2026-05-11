using LumStoreAPI.Core.Entities.DocumentEngine;
using LumStoreAPI.Core.Entities.DocumentTypes;

namespace LumStoreAPI.Core.Models.Systems;

public class NavItem
{
    public string Name { get; set; } = default!;
    public string? Url { get; set; }
    public string? Image { get; set; }
    public string? Target { get; set; }
    public IEnumerable<NavItem> Children { get; set; } = [];

    public static NavItem From(DocumentPage documentPage)
    {
        return documentPage switch
        {
            LinkListItem linkListItem => new NavItem(linkListItem),
            _ => new NavItem(documentPage)
        };
    }

    public NavItem(LinkListItem linkListItem)
    {
        this.Name = linkListItem.LinkListTitle;
        this.Url = linkListItem.LinkUrl;
        this.Children = linkListItem.Children.Select(From);
    }

    public NavItem(DocumentPage documentPage, string? image = "")
    {
        this.Name = documentPage.DocumentName;
        this.Url = documentPage.Node.ClassName.StartsWith("Pages.") ? documentPage.Node.RelativeUrl : null;
        this.Image = image;
        this.Target = "_self";
        this.Children = documentPage.Children.Select(From);
    }
}
