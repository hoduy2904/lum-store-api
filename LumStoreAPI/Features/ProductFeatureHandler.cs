using LumStoreAPI.Application.DTOs.ProductDTO;
using LumStoreAPI.Application.DTOs.ProductVariantDTO;
using LumStoreAPI.Application.FeatureQueries;
using LumStoreAPI.Application.Interfaces;
using LumStoreAPI.Core.Interfaces.Repositories;
using LumStoreAPI.Infrastructure.Repositories.Interfaces;
using MediatR;

namespace LumStoreAPI.Features;

public class ProductFeatureHandler(
    IMediaService mediaService,
    IProductVariantRepository productVariantRepository,
    IDiscountRuleRepository discountRuleRepository
) : IRequestHandler<ProductFeatureQuery, ProductClientDTO>
{
    public async Task<ProductClientDTO> Handle(ProductFeatureQuery request, CancellationToken cancellationToken)
    {
        var product = request.Product;

        var images = (product.Images.Length > 0
            ? await mediaService.GetMediaItemsAsync(product.Images)
            : []).OrderBy(x => product.Images.IndexOf(x.FileID));

        var variants = await productVariantRepository.GetProductVariantsAsync(v => v.ProductID == product.NodeID);
        var variantList = variants.ToList();

        IEnumerable<ProductVariantClientGetDTO> variantDTOs = [];
        if (variantList.Count > 0)
        {
            var variantImageGuids = variantList.SelectMany(v => v.Images).Distinct().ToArray();
            var variantImages = (variantImageGuids.Length > 0
                ? (await mediaService.GetMediaItemsAsync(variantImageGuids)).ToList()
                : []).OrderBy(x => variantImageGuids.IndexOf(x.FileID));

            variantDTOs = variantList.Select(v => new ProductVariantClientGetDTO(
                v,
                variantImages.Where(img => v.Images.Contains(img.FileID)).ToArray()
            ));
        }

        var discountRules = await discountRuleRepository.GetRulesAsync(product.NodeID, activeOnly: true);
        var discountTiers = discountRules.Select(r => new ProductDiscountTierDTO
        {
            RuleName = r.RuleName,
            MinQuantity = r.MinQuantity,
            MaxQuantity = r.MaxQuantity,
            DiscountAmount = r.DiscountAmount
        });

        return new ProductClientDTO(product)
        {
            Images = images.Select(i => i.FileURL).ToArray(),
            ProductVariants = variantDTOs,
            DiscountRules = discountTiers
        };
    }
}
