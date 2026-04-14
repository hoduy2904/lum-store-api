using System;
using LumStoreAPI.Application.DTOs.Widgets;
using LumStoreAPI.Core.Attributes;
using MediatR;

namespace LumStoreAPI.Application.Widgets;

[RegisterWidget("accordionWidget", typeof(AccordionWidget))]
public class AccordionWidget : IRequest<AccordionWidgetDTO>
{
    public int PathId { get; set; }
}
