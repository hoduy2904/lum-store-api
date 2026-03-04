using LumStoreAPI.Core.Entities.Systems;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LumStoreAPI.Infrastructure.Configurations
{
    internal class MediaLibraryConfiguration : IEntityTypeConfiguration<MediaLibrary>
    {
        public void Configure(EntityTypeBuilder<MediaLibrary> builder)
        {
            builder.HasKey(x => x.FileID);

            builder.HasIndex(x => x.FileName).IsUnique();

            builder.HasOne(x => x.MediaLibraryCategory)
                .WithMany(x => x.MediaLibraries)
                .HasForeignKey(x => x.CategoryID);
        }
    }
}
