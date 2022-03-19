using JBS.DataLayer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JBS.DataLayer.EntitiesConfiguration
{
    public class PostMetaConfiguration : BaseEntityTypeConfiguration<PostMeta, int>
    {
        public override void Config(EntityTypeBuilder<PostMeta> builder)
        {
            builder.ToTable("PostMeta");

            builder.Property(x => x.Key).HasMaxLength(50);
        }
    }
}
