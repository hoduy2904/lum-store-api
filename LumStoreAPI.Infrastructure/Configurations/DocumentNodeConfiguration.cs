using LumStoreAPI.Core.Entities.DocumentEngine;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LumStoreAPI.Infrastructure.Configurations
{
    internal class DocumentNodeConfiguration : IEntityTypeConfiguration<DocumentNode>
    {
        public void Configure(EntityTypeBuilder<DocumentNode> builder)
        {
            builder.HasKey(x => x.NodeID);

            builder.Property(x => x.NodeID).ValueGeneratedOnAdd();

            builder.HasIndex(x => new { x.NodeOrder, x.ParentNodeID });

            builder.HasIndex(x => new { x.ParentNodeID, x.NodeAlias }).IsUnique();

            builder.HasIndex(x => x.RelativeUrl);


            builder.Property(x => x.RelativeUrl)
            .HasMaxLength(300);

            builder.Property(x => x.ClassName)
                .HasMaxLength(50);

            builder.HasMany(x => x.Properties)
                .WithOne(x => x.Node)
                .HasForeignKey(x => x.NodeID);

            builder.HasOne(x => x.Parent)
                .WithMany(x => x.Childrens)
                .HasForeignKey(x => x.ParentNodeID);

            builder.HasMany(x => x.AncestorNodes)
                .WithOne(x => x.AncestorNode)
                .HasForeignKey(x => x.Ancestor);

            builder.HasMany(x => x.DescendantNodes)
                .WithOne(x => x.DescendantNode)
                .HasForeignKey(x => x.Descendant);

            builder.Property(x => x.NodeAlias)
                .HasMaxLength(200);
        }
    }
}
