using System.Linq.Expressions;
using LumStoreAPI.Core.Entities.DocumentTypes;
using LumStoreAPI.Core.Models.Riches;

namespace LumStoreAPI.Core.Interfaces.Repositories;

public interface IProductVariantRepository
{
    Task<ProductVariant?> GetProductVariantAsync(int variantId);
    Task<ProductVariant?> GetProductVariantAsync(string sku);
    Task<IEnumerable<ProductVariant>> GetProductVariantsAsync(Expression<Func<ProductVariant, bool>>? where = null);
    IQueryable<ProductVariant> GetProductVariants();
    Task<ProductVariant> InsertProductVariantAsync(ProductVariant productVariant);
    Task<int> UpdateProductVariantsAsync(Expression<Func<ProductVariant, bool>> condition, Action<SetterBuilder<ProductVariant>> action);
    Task<ProductVariant> UpdateProductVariantAsync(ProductVariant productVariant);
    Task<int> DeleteProductVariantsAsync(Expression<Func<ProductVariant, bool>> condition);
}
