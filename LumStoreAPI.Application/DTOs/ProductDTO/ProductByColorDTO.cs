namespace LumStoreAPI.Application.DTOs.ProductDTO;

public class ProductByColorDTO
{
    public string RelativeUrl { get; set; } = default!;
    public string NodeAlias { get; set; } = default!;
    public string ProductName { get; set; } = default!;
    public string? AvailableIn { get; set; }
    public string? Image { get; set; }
}
