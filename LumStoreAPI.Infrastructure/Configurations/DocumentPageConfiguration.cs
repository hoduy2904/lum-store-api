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
            builder.UseTptMappingStrategy();
        }
    }
}
