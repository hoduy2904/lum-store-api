using LumStoreAPI.Application.DTOs.ProductDTO;
using LumStoreAPI.Application.DTOs.ProductVariantDTO;
using LumStoreAPI.Application.FeatureQueries;
using LumStoreAPI.Application.Interfaces;
using LumStoreAPI.Core.Interfaces.Repositories;
using MediatR;

namespace LumStoreAPI.Features;

public class ProductFeatureHandler(
    IMediaService mediaService,
    IProductVariantRepository productVariantRepository
) : IRequestHandler<ProductFeatureQuery, ProductClientDTO>
{
    public async Task<ProductClientDTO> Handle(ProductFeatureQuery request, CancellationToken cancellationToken)
    {
        var product = request.Product;

        var images = product.Images.Length > 0
            ? await mediaService.GetMediaItemsAsync(product.Images)
            : [];

        var variants = await productVariantRepository.GetProductVariantsAsync(v => v.ProductID == product.NodeID);
        var variantList = variants.ToList();

        IEnumerable<ProductVariantGetDTO> variantDTOs = [];
        if (variantList.Count > 0)
        {
            var variantImageGuids = variantList.SelectMany(v => v.Images).Distinct().ToArray();
            var variantImages = variantImageGuids.Length > 0
                ? (await mediaService.GetMediaItemsAsync(variantImageGuids)).ToList()
                : [];

            variantDTOs = variantList.Select(v => new ProductVariantGetDTO(
                v,
                variantImages.Where(img => v.Images.Contains(img.FileID)).ToArray()
            ));
        }

        return new ProductClientDTO(product)
        {
            Images = images.Select(i => i.FileURL).ToArray(),
            ProductVariants = variantDTOs
        };
    }
}
