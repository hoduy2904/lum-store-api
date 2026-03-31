using System;
using LumStoreAPI.Application.DTOs.ProductVariantDTO;

namespace LumStoreAPI.Application.Interfaces;

public interface IProductVariantService
{
    Task<ProductVariantGetDTO?> GetProductVariantAsync(int variantId);
    Task<ProductVariantGetDTO?> GetProductVariantAsync(string sku);
    Task<IEnumerable<ProductVariantGetDTO>> GetProductVariantsAsync(int productId);
    Task<int> UpdateProductVariantAsync(int variantId, ProductVariantUpdateDTO request);
    Task<ProductVariantGetDTO> InsertProductVariantAsync(ProductVariantRequestDTO request);
    Task<int> DeleteProductVariantsAsync(int[] variantIds);
}
