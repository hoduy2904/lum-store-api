using LumStoreAPI.Core.Entities.Customers;

namespace LumStoreAPI.Libraries.Helpers
{
    public class OrderHelper
    {
        public static decimal CaculateUnitPrice(decimal price, int qty, IEnumerable<DiscountRule> discountRules)
        {
            var bestDiscount = discountRules
                .Where(x => x.MinQuantity <= qty)
                .OrderByDescending(x => x.MinQuantity)
                .FirstOrDefault();
            if (bestDiscount is null) return price;

            if (bestDiscount.DiscountAmount > 0) return (price - bestDiscount.DiscountAmount);
            return (price * (100 - bestDiscount.DiscountPercent) / 100);
        }
    }
}
