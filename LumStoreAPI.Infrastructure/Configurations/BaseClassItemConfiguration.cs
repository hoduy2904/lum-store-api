using LumStoreAPI.Core.Entities.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LumStoreAPI.Infrastructure.Configurations
{
    internal class BaseClassItemConfiguration : IEntityTypeConfiguration<BaseClassItem>
    {
        public void Configure(EntityTypeBuilder<BaseClassItem> builder)
        {
            builder.UseTpcMappingStrategy();
            builder.HasKey(x => x.ItemID);
        }
    }
}
