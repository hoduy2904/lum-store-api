using LumStoreAPI.Application.DTOs.Widgets;
using LumStoreAPI.Core.Attributes;
using MediatR;

namespace LumStoreAPI.Application.Widgets;

[RegisterWidget("linkListItemsWidget", typeof(LinkListWidget))]
public class LinkListWidget : IRequest<LinkListWidgetDTO>
{
    public int ItemPathId { get; set; }
}
