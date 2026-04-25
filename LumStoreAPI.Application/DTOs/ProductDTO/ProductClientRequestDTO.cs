using System;
using System.ComponentModel.DataAnnotations;

namespace LumStoreAPI.Application.DTOs.ProductDTO;

public class ProductClientRequestDTO
{
    public int Page { get; set; }
    [Range(6, 24)]
    public int PageSize { get; set; }
    public int CategoryId { get; set; }
    public string SortBy { get; set; } = "default";
    public int Price { get; set; }
    public string? Color { get; set; }
    public string? Search { get; set; }
}
