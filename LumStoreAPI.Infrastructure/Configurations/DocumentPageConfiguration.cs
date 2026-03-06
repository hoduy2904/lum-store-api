using LumStoreAPI.Core.Entities.DocumentEngine;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LumStoreAPI.Infrastructure.Configurations
{
    internal class DocumentPageConfiguration : IEntityTypeConfiguration<DocumentPage>
    {
        public void Configure(EntityTypeBuilder<DocumentPage> builder)
        {
            builder.HasKey(x => x.PageID);

            builder.Property(x => x.ClassName)
                .HasMaxLength(50);
            builder.Property(x => x.DocumentName)
                .HasMaxLength(100);

            builder.UseTptMappingStrategy();
        }
    }
}
