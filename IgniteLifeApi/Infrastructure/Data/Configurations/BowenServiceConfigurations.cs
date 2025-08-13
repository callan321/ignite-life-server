using IgniteLifeApi.Domain.Constants;
using IgniteLifeApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IgniteLifeApi.Infrastructure.Data.Configurations
{
    public class BowenServiceConfigurations : IEntityTypeConfiguration<BowenService>
    {
        public void Configure(EntityTypeBuilder<BowenService> builder)
        {
            builder.ToTable("BowenServices");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Title)
                   .IsRequired()
                   .HasMaxLength(FieldLengths.Title);

            builder.Property(x => x.Description)
                   .IsRequired();

            builder.Property(x => x.Price)
                   .HasColumnType("decimal(10,2)");

            builder.Property(x => x.DurationMinutes)
                   .IsRequired();

            builder.Property(x => x.ImageUrl)
                   .HasMaxLength(FieldLengths.Url);

            builder.Property(x => x.ImageAltText)
                   .HasMaxLength(FieldLengths.ImageAltText);

            builder.Property(x => x.IsActive)
                   .HasDefaultValue(true);

            // indexes
            builder.HasIndex(x => x.IsActive);
            builder.HasIndex(x => new { x.IsGroupSession, x.IsMultiSession });
        }
    }
}
