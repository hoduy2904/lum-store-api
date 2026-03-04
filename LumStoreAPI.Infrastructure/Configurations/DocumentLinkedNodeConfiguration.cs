using LumStoreAPI.Core.Entities.DocumentEngine;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LumStoreAPI.Infrastructure.Configurations
{
    internal class DocumentLinkedNodeConfiguration : IEntityTypeConfiguration<DocumentLinkedNode>
    {
        public void Configure(EntityTypeBuilder<DocumentLinkedNode> builder)
        {
            builder.HasKey(x => new { x.Ancestor, x.Descendant, x.Depth });
            builder.HasOne(x => x.AncestorNode)
                .WithMany(x=>x.AncestorNodes)
                .HasForeignKey(x => x.Ancestor)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.DescendantNode)
                .WithMany(x=>x.DescendantNodes)
                .HasForeignKey(x => x.Descendant)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
