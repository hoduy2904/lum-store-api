using System.ComponentModel.DataAnnotations;

namespace LumStoreAPI.Application.DTOs.ProductDTO;

public class CategoryProductsRequest
{
    public int Page { get; set; } = 1;

    [Range(1, 50, ErrorMessage = "pageSize cannot exceed 50")]
    public int PageSize { get; set; } = 9;

    /// <summary>newest | best_sellers | price_asc | price_desc</summary>
    public string SortBy { get; set; } = "newest";

    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
}
