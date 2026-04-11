using LumStoreAPI.Core.Entities.DocumentTypes;
using Microsoft.EntityFrameworkCore;

namespace LumStoreAPI.Infrastructure
{
    public partial class LumStoreContext
    {
        public DbSet<CTAImageItem> CTAImages { get; set; }
        public DbSet<LinkListItem> LinkLists { get; set; }
    }
}
