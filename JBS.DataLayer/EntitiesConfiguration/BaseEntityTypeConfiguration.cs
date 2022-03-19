using JBS.DataLayer.Entities.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JBS.DataLayer.EntitiesConfiguration
{
    public abstract class BaseEntityTypeConfiguration<T, TKey> : IEntityTypeConfiguration<T>
       where T : BaseEntity<TKey>
    {
        public abstract void Config(EntityTypeBuilder<T> builder);

        public void Configure(EntityTypeBuilder<T> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();


            Config(builder);

            builder.Property(x => x.CreatedDate).IsRequired().HasColumnType("DATETIME");
            builder.Property(x => x.CreatedBy).HasMaxLength(450);
            builder.Property(x => x.UpdatedDate).HasColumnType("DATETIME");
            builder.Property(x => x.UpdatedBy).HasMaxLength(450);
        }
    }
}
