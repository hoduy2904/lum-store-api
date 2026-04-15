using LumStoreAPI.Core.Entities.DocumentTypes;
using LumStoreAPI.Core.Entities.Integrations;
using Microsoft.EntityFrameworkCore;

namespace LumStoreAPI.Infrastructure
{
    public partial class LumStoreContext
    {
        public DbSet<CTAImageItem> CTAImages { get; set; }
        public DbSet<LinkListItem> LinkLists { get; set; }
        public DbSet<AccordionItem> AccordionItems { get; set; }


        public DbSet<ShiprelayDataSync> ShiprelayDataSyncs { get; set; }
    }
}
