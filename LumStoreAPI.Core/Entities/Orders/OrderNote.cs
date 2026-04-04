using LumStoreAPI.Core.Entities.Base;
using LumStoreAPI.Core.Entities.Systems;

namespace LumStoreAPI.Core.Entities.Orders;

/// <summary>Internal notes added by staff to an order (not visible to customer).</summary>
public class OrderNote : BaseClassItem
{
    public int OrderId { get; set; }
    public string Note { get; set; } = default!;
    public int AuthorId { get; set; }
    public string AuthorName { get; set; } = default!;

    public virtual Order Order { get; set; } = default!;
    public virtual User Author { get; set; } = default!;
}
