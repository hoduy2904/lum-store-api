using LumStoreAPI.Core.Entities.Pages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LumStoreAPI.Infrastructure.Configurations
{
    internal class GeneralContentConfiguration : IEntityTypeConfiguration<GeneralContent>
    {
        public void Configure(EntityTypeBuilder<GeneralContent> builder)
        {
            builder.Property(x => x.Title)
           .HasMaxLength(100);

            builder.Property(x => x.Descrition)
            .HasMaxLength(200);
        }
    }
}
