using LumStoreAPI.Application.DTOs.StoreDTO;

namespace LumStoreAPI.Application.DTOs.Widgets;

public record class ProductColorWidgetDTO(string? Title, string? Description, IEnumerable<RelatedContentKeyValue> Colors);
