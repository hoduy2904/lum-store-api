using System.ComponentModel.DataAnnotations;

namespace LumStoreAPI.Application.DTOs.ProductComboDTO
{
    public record class ProductComboRequest
    (
        [Range(1, int.MaxValue)]
        int ProductId,
         [Range(1, int.MaxValue)]
        int VariantId
    );
}
