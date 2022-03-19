using JBS.DataLayer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JBS.DataLayer.EntitiesConfiguration
{
    public class PostTagConfiguration : BaseEntityTypeConfiguration<PostTag, int>
    {
        public override void Config(EntityTypeBuilder<PostTag> builder)
        {
            builder.ToTable("PostTag");

            builder.HasOne(x => x.Tag)
                   .WithMany(x => x.PostTags)
                   .HasForeignKey(x => x.TagId);
            builder.HasOne(x => x.Post)
                   .WithMany(x => x.PostTags)
                   .HasForeignKey(x => x.PostId);
        }
    }
}
