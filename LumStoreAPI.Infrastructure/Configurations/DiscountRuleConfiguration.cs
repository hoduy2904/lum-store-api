using LumStoreAPI.Core.Entities.Customers;
using LumStoreAPI.Core.Entities.DocumentTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LumStoreAPI.Infrastructure.Configurations;

internal class DiscountRuleConfiguration : IEntityTypeConfiguration<DiscountRule>
{
    public void Configure(EntityTypeBuilder<DiscountRule> builder)
    {
        builder.HasIndex(x => x.IsActive);

        builder.Property(x => x.RuleName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.DiscountPercent).HasPrecision(5, 2).IsRequired();
        builder.Property(x => x.DiscountAmount).HasPrecision(18, 2).IsRequired();
    }
}

internal class DiscountRuleMappingConfiguration : IEntityTypeConfiguration<DiscountRuleMapping>
{
    public void Configure(EntityTypeBuilder<DiscountRuleMapping> builder)
    {
        builder.HasKey(x => new { x.ProductId, x.DiscountRuleId });

        builder.HasOne(x => x.DiscountRule)
            .WithMany(x => x.DiscountRuleMappings)
            .HasForeignKey(x => x.DiscountRuleId);

    }
}