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
            var count = await _lumStoreContext.ProductCombos
                .Where(x => productCombos.Any(c => c.ProductID == x.ProductID && c.VariantID == x.VariantID))
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
                 .Include(x => x.Product)
                 .Include(x => x.ProductVariant)
                 .ThenInclude(x => x.Product)
                 .AsNoTrackingWithIdentityResolution()
                 .Select(x => new ProductRelated
                 {
                     ProductId = productId,
                     ProductVariantName = x.ProductVariant!.Product!.ProductName,
                     VariantId = x.VariantID,
                     VariantName = x.ProductVariant.VariantName
                 }).ToArrayAsync();

        }

        public async Task<ProductCombo> InsertProductCombo(ProductCombo productCombo)
        {
            _lumStoreContext.ProductCombos.Add(productCombo);
            await _lumStoreContext.SaveChangesAsync();
            return productCombo;
        }
    }
}
