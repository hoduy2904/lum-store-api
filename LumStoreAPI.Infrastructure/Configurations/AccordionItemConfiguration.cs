using LumStoreAPI.Core.Entities.DocumentTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LumStoreAPI.Infrastructure.Configurations
{
    internal class AccordionItemConfiguration : IEntityTypeConfiguration<AccordionItem>
    {
        public void Configure(EntityTypeBuilder<AccordionItem> builder)
        {
            builder.Property(x => x.Title).HasMaxLength(100);

            builder.Property(x => x.Description).HasMaxLength(250);
        }
    }
}
