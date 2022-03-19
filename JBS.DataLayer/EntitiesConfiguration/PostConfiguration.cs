using JBS.DataLayer.Constants;
using JBS.DataLayer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations.Schema;

namespace JBS.DataLayer.EntitiesConfiguration
{
    public class PostConfiguration : BaseEntityTypeConfiguration<Post, Guid>
    {
        public override void Config(EntityTypeBuilder<Post> builder)
        {
            builder.ToTable("Post");
            builder.HasKey(x => x.Id);


            builder.HasOne(x => x.Author)
                   .WithMany(x => x.Posts)
                   .HasForeignKey(x => x.AuthorId);

            builder.Property(x => x.Title)
                   .HasMaxLength(EntityConstants.TitleLength);
            builder.Property(x => x.MetaTitle)
                   .HasMaxLength(EntityConstants.MetaTitleLength);
            builder.Property(x => x.Slug)
                   .HasMaxLength(EntityConstants.SlugLength);

        }
    }
}
