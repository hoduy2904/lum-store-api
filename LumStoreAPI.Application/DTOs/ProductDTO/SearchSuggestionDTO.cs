namespace LumStoreAPI.Application.DTOs.ProductDTO;

public class SearchSuggestionDTO
{
    public string NodeAlias { get; set; } = default!;
    public SearchSuggestionFieldsDTO Fields { get; set; } = default!;
}

public class SearchSuggestionFieldsDTO
{
    public string ProductName { get; set; } = default!;
    public string[] Images { get; set; } = [];
    public decimal Price { get; set; }
    public decimal? PriceDiscount { get; set; }
    public string? CategoryName { get; set; }
}
