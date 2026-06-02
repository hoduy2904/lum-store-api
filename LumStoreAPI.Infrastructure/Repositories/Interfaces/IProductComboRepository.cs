using LumStoreAPI.Core.Entities.DocumentTypes;
using LumStoreAPI.Core.Models.Systems;

namespace LumStoreAPI.Infrastructure.Repositories.Interfaces
{
    public interface IProductComboRepository
    {
        Task<IEnumerable<ProductRelated>> GetProductRelatedsAsync(int productId);
        Task<IEnumerable<ComboItemPricing>> GetComboItemsForPricingAsync(IEnumerable<int> productIds);
        Task<ProductCombo> InsertProductCombo(ProductCombo productCombo);
        Task<bool> DeleteProductCombo(ProductCombo productCombo);
        Task<bool> DeleteProductCombos(params ProductCombo[] productCombos);
        Task<bool> DeleteProductCombos(int productId);
    }
}
