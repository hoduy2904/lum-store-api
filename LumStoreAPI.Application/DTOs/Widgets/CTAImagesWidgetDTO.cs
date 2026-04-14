using System;
using LumStoreAPI.Application.DTOs.DocumentContents;

namespace LumStoreAPI.Application.DTOs.Widgets;

public class CTAImagesWidgetDTO
{
    public IEnumerable<CTAImageItemDTO> CTAImages { get; set; } = [];
}
