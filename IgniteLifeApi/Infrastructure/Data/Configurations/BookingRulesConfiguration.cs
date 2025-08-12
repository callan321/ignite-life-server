using IgniteLifeApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IgniteLifeApi.Infrastructure.Data.Configurations;

public class BookingRulesConfiguration : IEntityTypeConfiguration<BookingRules>
{
    public void Configure(EntityTypeBuilder<BookingRules> builder)
    {
        // UNIQUE filtered index on IsDefault = true (PostgreSQL syntax with quoted column)
        builder.HasIndex(br => br.IsDefault)
               .IsUnique()
               .HasFilter("\"IsDefault\" = TRUE");

        builder.HasMany(r => r.OpeningHours)
               .WithOne()
               .HasForeignKey(h => h.BookingRulesId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
