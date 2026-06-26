using LumStoreAPI.Application.DTOs.ProductDTO;
using LumStoreAPI.Application.DTOs.ProductVariantDTO;
using LumStoreAPI.Application.FeatureQueries;
using LumStoreAPI.Application.Interfaces;
using LumStoreAPI.Core.Interfaces.Repositories;
using LumStoreAPI.Core.Interfaces.Services;
using MediatR;

namespace LumStoreAPI.Features;

public class ProductFeatureHandler(
    IMediaService mediaService,
    IProductVariantRepository productVariantRepository,
    IDiscountRuleService discountRuleService,
    ISettingKeyValueService settingKeyValueService
) : IRequestHandler<ProductFeatureQuery, ProductClientDTO>
{
    public async Task<ProductClientDTO> Handle(ProductFeatureQuery request, CancellationToken cancellationToken)
    {
        var product = request.Product;

        var images = (product.Images.Length > 0
            ? await mediaService.GetMediaItemsAsync(product.Images)
            : []).OrderBy(x => product.Images.IndexOf(x.FileID)).ToArray();

        //var productImageIds = new HashSet<Guid>(product.Images);

        var variants = await productVariantRepository.GetProductVariantsAsync(v => v.ProductID == product.NodeID);
        var variantList = variants.ToList();

        IEnumerable<ProductVariantClientGetDTO> variantDTOs = [];
        if (variantList.Count > 0)
        {
            var variantImageGuids = variantList.SelectMany(v => v.Images).Distinct().ToArray();
            var variantImages = (variantImageGuids.Length > 0
                ? (await mediaService.GetMediaItemsAsync(variantImageGuids)).ToList()
                : []).OrderBy(x => variantImageGuids.IndexOf(x.FileID)).ToList();

            var colorImageIds = variantList
                .Where(v => v.Color?.ColorImageId.HasValue == true)
                .Select(v => v.Color!.ColorImageId!.Value)
                .Distinct().ToArray();
            var colorImageMap = colorImageIds.Length > 0
                ? (await mediaService.GetMediaItemsAsync(colorImageIds)).ToDictionary(m => m.FileID, m => m.FileURL)
                : new Dictionary<Guid, string>();

            variantDTOs = variantList.Select(v =>
            {
                var variantOnlyImages = variantImages
                    .Where(img => v.Images.Contains(img.FileID))
                    //.Where(img => v.Images.Contains(img.FileID) && !productImageIds.Contains(img.FileID))
                    .OrderBy(img => v.Images.IndexOf(img.FileID))
                    .ToArray();

                string? colorImageUrl = v.Color?.ColorImageId.HasValue == true && colorImageMap.TryGetValue(v.Color.ColorImageId.Value, out var cu) ? cu : null;
                //return new ProductVariantClientGetDTO(v, [.. images, .. uniqueVariantImages], colorImageUrl);
                return new ProductVariantClientGetDTO(v, variantOnlyImages, colorImageUrl);
            });
        }

        var discountRules = await discountRuleService.GetRulesAsync(product.NodeID, activeOnly: true);
        var discountTiers = discountRules.Select(r => new ProductDiscountTierDTO
        {
            RuleName = r.RuleName,
            MinQuantity = r.MinQuantity,
            MaxQuantity = r.MaxQuantity,
            DiscountPercent = r.DiscountPercent,
            DiscountAmount = r.DiscountAmount
        });

        var accordions = await settingKeyValueService.GetSettingContentsAsync("Product_Accordion");

        var tiersArray = discountTiers.ToArray();
        var dto = new ProductClientDTO(product)
        {
            Images = images.Select(i => i.FileURL).ToArray(),
            ProductVariants = variantDTOs,
            DiscountRules = tiersArray,
            Accordions = accordions,
            DiscountedPrice = (product.IsCombo || product.IsExpand) ? null : ComputeDiscountedPrice(product.Price, tiersArray),
        };

        if (product.IsCombo || product.IsExpand)
        {
            var comboResult = await discountRuleService.CalculateComboPriceAsync(product.NodeID);

            if (product.IsCombo)
            {
                dto.Price = comboResult.SubTotal;
                dto.PriceDiscount = comboResult.TotalPrice < comboResult.SubTotal
                    ? comboResult.TotalPrice
                    : null;
            }
            else // IsExpand: comboItems are selectable options — price reflects a single item
            {
                var firstItem = comboResult.Items.FirstOrDefault();
                dto.Price = firstItem?.UnitPrice ?? product.Price;
                dto.PriceDiscount = firstItem != null && firstItem.DiscountedPrice < firstItem.UnitPrice
                    ? firstItem.DiscountedPrice
                    : null;
            }

            dto.DiscountRules = [];
            dto.ComboItems = comboResult.Items;
            dto.ComboStock = comboResult.ComboStock;
        }

        return dto;
    }

    private static decimal? ComputeDiscountedPrice(decimal? price, IEnumerable<ProductDiscountTierDTO> tiers)
    {
        if (price is null or <= 0) return null;
        var best = tiers
            .Where(t => t.MinQuantity <= 1 && (t.MaxQuantity == null || t.MaxQuantity >= 1))
            .OrderByDescending(t => t.DiscountPercent + t.DiscountAmount)
            .FirstOrDefault();
        if (best == null) return null;
        return Math.Max(0, Math.Round(price.Value * (1 - best.DiscountPercent / 100) - best.DiscountAmount, 2));
    }
}
