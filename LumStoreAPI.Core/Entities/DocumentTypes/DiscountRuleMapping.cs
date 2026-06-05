using LumStoreAPI.Core.Entities.Customers;

namespace LumStoreAPI.Core.Entities.DocumentTypes
{
    public class DiscountRuleMapping
    {
        public int ProductId { get; set; }
        public int DiscountRuleId { get; set; }
        public virtual DiscountRule DiscountRule { get; set; } = default!;
    }
}
