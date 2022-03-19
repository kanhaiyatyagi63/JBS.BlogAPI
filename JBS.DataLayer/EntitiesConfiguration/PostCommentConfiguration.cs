using JBS.DataLayer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JBS.DataLayer.EntitiesConfiguration
{
    public class PostCommentConfiguration : BaseEntityTypeConfiguration<PostComment, int>
    {
        public override void Config(EntityTypeBuilder<PostComment> builder)
        {
            builder.ToTable("PostComment");

        }
    }
}
