using JBS.DataLayer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JBS.DataLayer.EntitiesConfiguration
{
    public class PostCategoryConfiguration : BaseEntityTypeConfiguration<PostCategory, int>
    {
        public override void Config(EntityTypeBuilder<PostCategory> builder)
        {
            builder.ToTable("PostCategory");

            builder.HasOne(x => x.Category)
                   .WithMany(x => x.PostCategories)
                   .HasForeignKey(x => x.CategoryId);
            builder.HasOne(x => x.Post)
                   .WithMany(x => x.PostCategories)
                   .HasForeignKey(x => x.PostId);
        }
    }
}
