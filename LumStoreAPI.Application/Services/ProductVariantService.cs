using LumStoreAPI.Application.DTOs.MediaDTO;
using LumStoreAPI.Application.DTOs.ProductVariantDTO;
using LumStoreAPI.Application.Interfaces;
using LumStoreAPI.Core.Interfaces.Repositories;

namespace LumStoreAPI.Application.Services;

internal class ProductVariantService : IProductVariantService
{
    private readonly IProductVariantRepository _productVariantRepository;
    private readonly IMediaService _mediaService;
    public ProductVariantService(IProductVariantRepository productVariantRepository, IMediaService mediaService)
    {
        _productVariantRepository = productVariantRepository;
        _mediaService = mediaService;
    }
    public Task<int> DeleteProductVariantsAsync(int[] variantIds)
    {
        return _productVariantRepository.DeleteProductVariantsAsync(x => variantIds.Contains(x.ItemID));
    }

    public async Task<ProductVariantGetDTO?> GetProductVariantAsync(int variantId)
    {
        var productVariant = await _productVariantRepository.GetProductVariantAsync(variantId);
        if (productVariant == null)
            return null;
        var mediaItems = Array.Empty<MediaItemDTO>();
        if (productVariant.Images.Any())
        {
            mediaItems = (await _mediaService.GetMediaItemsAsync(productVariant.Images)).ToArray();
        }
        return new ProductVariantGetDTO(productVariant, mediaItems);
    }

    public async Task<ProductVariantGetDTO?> GetProductVariantAsync(string sku)
    {
        var productVariant = await _productVariantRepository.GetProductVariantAsync(sku);

        if (productVariant == null) return null;
        var mediaItems = (await _mediaService.GetMediaItemsAsync(productVariant.Images)).ToArray();
        return new ProductVariantGetDTO(productVariant, mediaItems);
    }

    public async Task<IEnumerable<ProductVariantGetDTO>> GetProductVariantsAsync(int productId)
    {
        var productVariants = await _productVariantRepository.GetProductVariantsAsync(x => x.ProductID == productId);
        if (!productVariants.Any()) return Enumerable.Empty<ProductVariantGetDTO>();

        var mediaItems = await _mediaService.GetMediaItemsAsync(productVariants.SelectMany(x => x.Images).ToArray());

        return productVariants.Select(x => new ProductVariantGetDTO(x, mediaItems.Where(m => x.Images.Contains(m.FileID)).ToArray()));
    }

    public async Task<ProductVariantGetDTO> InsertProductVariantAsync(ProductVariantRequestDTO request)
    {
        var entity = await _productVariantRepository.InsertProductVariantAsync(request.GetEntity());
        var mediaItems = await _mediaService.GetMediaItemsAsync(entity.Images);
        return new ProductVariantGetDTO(entity, mediaItems.ToArray());
    }

    public Task<int> UpdateProductVariantAsync(int variantId, ProductVariantUpdateDTO request)
    {
        return _productVariantRepository.UpdateProductVariantsAsync(x => x.ItemID == variantId,
        x => x.Set(p => p.Color, request.Color)
        .Set(p => p.Images, request.Images)
        .Set(p => p.SKU, request.SKU)
        .Set(p => p.UPC, request.UPC)
        .Set(p => p.VariantName, request.VariantName)
        );
    }
}
