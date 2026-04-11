using System;
using LumStoreAPI.Application.DTOs.DocumentPageDTO;
using LumStoreAPI.Application.DTOs.ProductDTO;
using LumStoreAPI.Core.Models.Controls;

namespace LumStoreAPI.Application.DTOs.Widgets;

public class OutStandingProductWidgetDTO
{
    public string Type { get; set; } = default!;
    public string? Pretitle { get; set; }
    public string? CTAHeader { get; set; }
    public string? CTADescription { get; set; }
    public string[] CTAImages { get; set; } = [];
    public LinkControl? CTALink { get; set; }
    public IEnumerable<DocumentClientGetDTO<ProductClientDTO>> Products { get; set; } = [];
}
