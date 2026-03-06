using LumStoreAPI.Core.Entities.Pages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LumStoreAPI.Infrastructure.Configurations
{
    internal class HomePageConfiguration : IEntityTypeConfiguration<HomePage>
    {
        public void Configure(EntityTypeBuilder<HomePage> builder)
        {
            builder.Property(x => x.PageTitle)
                .HasMaxLength(100);
        }
    }
}
