using LumStoreAPI.Application.DTOs.MediaDTO;
using LumStoreAPI.Application.DTOs.ProductVariantDTO;
using LumStoreAPI.Application.DTOs.StoreDTO;
using LumStoreAPI.Application.Interfaces;
using LumStoreAPI.Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LumStoreAPI.Application.Services;

internal class ProductVariantService : IProductVariantService
{
    private readonly IProductVariantRepository _productVariantRepository;
    private readonly IMediaService _mediaService;
    private readonly IShiprelaySystemRespository _shiprelaySystemRespository;
    public ProductVariantService(IProductVariantRepository productVariantRepository, IMediaService mediaService, IShiprelaySystemRespository shiprelaySystemRespository)
    {
        _productVariantRepository = productVariantRepository;
        _mediaService = mediaService;
        _shiprelaySystemRespository = shiprelaySystemRespository;
    }
    public async Task<int> DeleteProductVariantsAsync(int[] variantIds)
    {
        var count = await _productVariantRepository.DeleteProductVariantsAsync(x => variantIds.Contains(x.ItemID));
        if (count > 0)
        {
            await _shiprelaySystemRespository.SyncVariantShiprelayAsync(variantIds.ToArray(), Core.Models.Enums.EntryActionStatus.DELETE);
        }

        return count;
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

    public async Task<IEnumerable<ContentKeyValue>> GetProductVariantColorsAsync(int MaxColor)
    {
        var colors = _productVariantRepository.GetProductVariants()
             .Select(x => new ContentKeyValue
             {
                 Key = x.VariantName,
                 Value = x.Color
             })
             .Distinct();

        if (MaxColor > 0)
        {
            colors = colors.Take(MaxColor);
        }

        return await colors.ToArrayAsync();
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
        await _shiprelaySystemRespository.SyncProductShiprelayAsync(entity.ProductID, Core.Models.Enums.EntryActionStatus.INSERT, entity.ItemID);
        return new ProductVariantGetDTO(entity, mediaItems.ToArray());
    }

    public async Task<int> UpdateProductVariantAsync(int variantId, ProductVariantUpdateDTO request)
    {

        var count = await _productVariantRepository.UpdateProductVariantsAsync(x => x.ItemID == variantId,
        x => x.Set(p => p.ColorId, request.ColorId)
        .Set(p => p.Images, request.Images)
        .Set(p => p.SKU, request.SKU)
        .Set(p => p.UPC, request.UPC)
        .Set(p => p.VariantName, request.VariantName)
        );

        if (count > 0)
        {
            await _shiprelaySystemRespository.SyncVariantShiprelayAsync(variantId, Core.Models.Enums.EntryActionStatus.UPDATE);
        }
        return count;
    }
}
