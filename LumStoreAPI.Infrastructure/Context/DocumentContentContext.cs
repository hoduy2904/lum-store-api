using LumStoreAPI.Core.Entities.Customers;
using LumStoreAPI.Core.Entities.DocumentTypes;
using LumStoreAPI.Core.Entities.Integrations;
using LumStoreAPI.Core.Entities.Systems;
using Microsoft.EntityFrameworkCore;

namespace LumStoreAPI.Infrastructure
{
    public partial class LumStoreContext
    {
        public DbSet<CTAImageItem> CTAImages { get; set; }
        public DbSet<LinkListItem> LinkLists { get; set; }
        public DbSet<AccordionItem> AccordionItems { get; set; }


        public DbSet<ShiprelayDataSync> ShiprelayDataSyncs { get; set; }
        public DbSet<CustomerAddress> CustomerAddresses { get; set; }
        public DbSet<ColorCategory> ColorCategories { get; set; }
        public DbSet<ColorItem> ColorItems { get; set; }
    }
}
