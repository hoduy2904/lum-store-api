using LumStoreAPI.Core.Entities.Systems;

namespace LumStoreAPI.Application.DTOs.ColorDTO;

public class ColorInsertRequest
{
    public string ColorName { get; set; } = default!;
    public string? ColorValue { get; set; }
    public Guid? ColorImageId { get; set; }
    public int CategoryId { get; set; }

    public ColorItem GetEntity()
    {
        return new ColorItem
        {
            CategoryId = this.CategoryId,
            ColorName = this.ColorName,
            ColorValue = this.ColorValue,
            ColorImageId = this.ColorImageId,
        };
    }
}
