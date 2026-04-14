using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace LumStoreAPI.Infrastructure.Helpers
{
    public class ValueCompareHelper
    {
        public static ValueComparer<Guid[]> GUIDArrayCompare => new ValueComparer<Guid[]>(
                (c1, c2) => c1.SequenceEqual(c2),
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => c.ToArray());
    }
}
