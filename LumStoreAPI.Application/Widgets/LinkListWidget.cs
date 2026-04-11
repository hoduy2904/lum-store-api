using LumStoreAPI.Application.DTOs.Widgets;
using MediatR;

namespace LumStoreAPI.Application.Widgets;

public class LinkListWidget : IRequest<LinkListWidgetDTO>
{
    public int ItemPathId { get; set; }
}
