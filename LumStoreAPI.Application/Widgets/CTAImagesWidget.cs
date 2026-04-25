using System;
using LumStoreAPI.Application.DTOs.Widgets;
using LumStoreAPI.Core.Attributes;
using MediatR;

namespace LumStoreAPI.Application.Widgets;

[RegisterWidget("ctaImagesWidget", typeof(CTAImagesWidget))]
public class CTAImagesWidget : IRequest<CTAImagesWidgetDTO>
{
    public int PathId { get; set; }
}
