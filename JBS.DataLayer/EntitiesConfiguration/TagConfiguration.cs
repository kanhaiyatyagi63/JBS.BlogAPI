using JBS.DataLayer.Constants;
using JBS.DataLayer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JBS.DataLayer.EntitiesConfiguration
{
    public class TagConfiguration : BaseEntityTypeConfiguration<Tag, int>
    {
        public override void Config(EntityTypeBuilder<Tag> builder)
        {
            builder.ToTable("Tag");

            builder.Property(x => x.Title)
                  .HasMaxLength(EntityConstants.TitleLength)
                  .IsRequired();
            builder.Property(x => x.MetaTitle)
                   .HasMaxLength(EntityConstants.MetaTitleLength);
            builder.Property(x => x.Slug)
                   .HasMaxLength(EntityConstants.SlugLength);
        }
    }
}
