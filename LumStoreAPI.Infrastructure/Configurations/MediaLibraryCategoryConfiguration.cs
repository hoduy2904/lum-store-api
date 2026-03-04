using LumStoreAPI.Core.Entities.Systems;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LumStoreAPI.Infrastructure.Configurations
{
    internal class MediaLibraryCategoryConfiguration : IEntityTypeConfiguration<MediaLibraryCategory>
    {
        public void Configure(EntityTypeBuilder<MediaLibraryCategory> builder)
        {
            builder.HasKey(x => x.CategoryID);

            builder.HasIndex(x => x.CategoryName)
                .IsUnique();

            builder.HasIndex(x => x.FolderName)
               .IsUnique();

            builder.HasMany(x => x.MediaLibraries)
                .WithOne(x => x.MediaLibraryCategory)
                .HasForeignKey(x => x.CategoryID);
        }
    }
}
