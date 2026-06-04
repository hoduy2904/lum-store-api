using LumStoreAPI.Core.Entities.DocumentTypes;
using LumStoreAPI.Core.Models.Systems;
using LumStoreAPI.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LumStoreAPI.Infrastructure.Repositories.Presentations
{
    internal class ProductComboRepository(
        LumStoreContext lumStoreContext
        ) : IProductComboRepository
    {
        private readonly LumStoreContext _lumStoreContext = lumStoreContext;
        public Task<bool> DeleteProductCombo(ProductCombo productCombo)
        {
            return DeleteProductCombos(productCombo);
        }

        public async Task<bool> DeleteProductCombos(params ProductCombo[] productCombos)
        {
            if (productCombos.Length == 0) return false;

            var variantIds = productCombos.Select(x => x.VariantID).Distinct();
            var productIds = productCombos.Select(x => x.ProductID).Distinct();
            var count = await _lumStoreContext.ProductCombos
                .Where(x => variantIds.Contains(x.VariantID) && productIds.Contains(x.ProductID))
                .ExecuteDeleteAsync();

            return count > 0;
        }

        public async Task<bool> DeleteProductCombos(int productId)
        {
            var count = await _lumStoreContext.ProductCombos
                .Where(x => x.ProductID == productId)
                .ExecuteDeleteAsync();

            return count > 0;
        }

        public async Task<IEnumerable<ProductRelated>> GetProductRelatedsAsync(int productId)
        {
            return await _lumStoreContext.ProductCombos
                 .Where(x => x.ProductID == productId)
                 .Include(x => x.ProductVariant)
                 .ThenInclude(x => x.Product)
                 .AsNoTrackingWithIdentityResolution()
                 .Select(x => new ProductRelated
                 {
                     ProductId = x.ProductID,
                     ProductVariantName = x.ProductVariant!.Product!.ProductName,
                     VariantId = x.VariantID,
                     VariantName = x.ProductVariant.VariantName,
                     Price = x.ProductVariant.Product!.Price,
                     PriceDiscount = x.ProductVariant.Product!.PriceDiscount,
                 }).ToArrayAsync();
        }

        public async Task<IEnumerable<ComboItemPricing>> GetComboItemsForPricingAsync(IEnumerable<int> productIds)
        {
            var ids = productIds.ToList();
            if (ids.Count == 0) return [];

            return await _lumStoreContext.ProductCombos
                .Where(x => ids.Contains(x.ProductID))
                .Include(x => x.ProductVariant)
                .ThenInclude(v => v.Product)
                .AsNoTrackingWithIdentityResolution()
                .Select(x => new ComboItemPricing
                {
                    ProductId = x.ProductID,
                    VariantId = x.VariantID,
                    VariantName = x.ProductVariant!.VariantName,
                    SubProductNodeId = x.ProductVariant.ProductID,
                    SubProductName = x.ProductVariant.Product!.ProductName,
                    SubProductPrice = x.ProductVariant.Product!.Price,
                    SubProductPriceDiscount = x.ProductVariant.Product!.PriceDiscount,
                    Stock = x.ProductVariant.Stock,
                }).ToListAsync();
        }

        public async Task<ProductCombo> InsertProductCombo(ProductCombo productCombo)
        {
            _lumStoreContext.ProductCombos.Add(productCombo);
            await _lumStoreContext.SaveChangesAsync();
            return productCombo;
        }
    }
}
