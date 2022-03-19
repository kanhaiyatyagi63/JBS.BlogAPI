using JBS.DataLayer.Constants;
using JBS.DataLayer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JBS.DataLayer.EntitiesConfiguration
{
    public class CategoryConfiguration : BaseEntityTypeConfiguration<Category, int>
    {
        public override void Config(EntityTypeBuilder<Category> builder)
        {
            builder.ToTable("Category");

            builder.Property(x => x.Title)
                  .HasMaxLength(EntityConstants.TitleLength);
            builder.Property(x => x.MetaTitle)
                   .HasMaxLength(EntityConstants.MetaTitleLength);
            builder.Property(x => x.Slug)
                   .HasMaxLength(EntityConstants.SlugLength);
        }
    }
}
