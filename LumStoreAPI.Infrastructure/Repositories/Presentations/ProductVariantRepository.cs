using System.Linq.Expressions;
using LumStoreAPI.Core.Entities.DocumentTypes;
using LumStoreAPI.Core.Interfaces.Repositories;
using LumStoreAPI.Core.Models.Riches;
using Microsoft.EntityFrameworkCore;

namespace LumStoreAPI.Infrastructure.Repositories.Presentations;

internal class ProductVariantRepository : IProductVariantRepository
{
    private readonly LumStoreContext _lumStoreContext;
    public ProductVariantRepository(LumStoreContext lumStoreContext)
    {
        _lumStoreContext = lumStoreContext;
    }
    public Task<int> DeleteProductVariantsAsync(Expression<Func<ProductVariant, bool>> condition)
    {
        return _lumStoreContext.ProductVariants
            .Where(condition).ExecuteDeleteAsync();
    }

    public Task<ProductVariant?> GetProductVariantAsync(int variantId)
    {
        return _lumStoreContext.ProductVariants
        .Include(x => x.Color)
        .AsNoTrackingWithIdentityResolution()
        .FirstOrDefaultAsync(x => x.ItemID == variantId);
    }

    public Task<ProductVariant?> GetProductVariantAsync(string sku)
    {
        return _lumStoreContext.ProductVariants
        .Include(x => x.Color)
        .AsNoTrackingWithIdentityResolution()
        .FirstOrDefaultAsync(x => x.SKU.Equals(sku));
    }

    public IQueryable<ProductVariant> GetProductVariants()
    {
        return _lumStoreContext.ProductVariants;
    }

    public async Task<IEnumerable<ProductVariant>> GetProductVariantsAsync(Expression<Func<ProductVariant, bool>>? where = null)
    {
        where ??= x => true;
        return await _lumStoreContext.ProductVariants
        .Include(x => x.Color)
        .AsNoTrackingWithIdentityResolution().Where(where).ToArrayAsync();
    }

    public async Task<ProductVariant> InsertProductVariantAsync(ProductVariant productVariant)
    {
        await _lumStoreContext.Set<ProductVariant>().AddAsync(productVariant);
        await _lumStoreContext.SaveChangesAsync();
        return productVariant;
    }

    public async Task<ProductVariant> UpdateProductVariantAsync(ProductVariant productVariant)
    {
        _lumStoreContext.Set<ProductVariant>().Update(productVariant);
        await _lumStoreContext.SaveChangesAsync();
        return productVariant;
    }

    public Task<int> UpdateProductVariantsAsync(Expression<Func<ProductVariant, bool>> condition, Action<SetterBuilder<ProductVariant>> action)
    {
        var builder = new SetterBuilder<ProductVariant>();
        action.Invoke(builder);

        return _lumStoreContext.Set<ProductVariant>()
             .Where(condition)
             .ExecuteUpdateAsync(x =>
             {
                 foreach (var bd in builder.GetValues())
                 {
#pragma warning disable EF1001 // Internal EF Core API usage.
                     x.SetProperty(bd.property, bd.value);
                 }
             });
    }
}
