using IgniteLifeApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IgniteLifeApi.Infrastructure.Data.Configurations
{
    public class BookingRulesConfiguration : IEntityTypeConfiguration<BookingRules>
    {
        public void Configure(EntityTypeBuilder<BookingRules> builder)
        {
            builder.ToTable("booking_rules");

            builder.HasKey(r => r.Id);

            builder.Property(r => r.TimeZoneId)
                   .IsRequired()
                   .HasMaxLength(64);

            builder.Property(r => r.MaxAdvanceBookingDays)
                   .IsRequired();

            builder.Property(r => r.BufferBetweenBookingsMinutes)
                   .IsRequired();

            builder.Property(r => r.SlotDurationMinutes)
                   .IsRequired();

            builder.Property(r => r.MinAdvanceBookingHours)
                   .IsRequired();

            builder.Property(r => r.CreatedAtUtc)
                   .HasColumnType("timestamptz");

            builder.Property(r => r.UpdatedAtUtc)
                   .HasColumnType("timestamptz");

            // Relationships
            builder.HasMany(r => r.OpeningHours)
                   .WithOne(o => o.BookingRules)
                   .HasForeignKey(o => o.BookingRulesId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(r => r.BlockedPeriods)
                   .WithOne(b => b.BookingRules)
                   .HasForeignKey(b => b.BookingRulesId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Singleton constraint (PostgreSQL syntax)
            builder.HasIndex(r => r.Id)
                   .IsUnique()
                   .HasFilter("\"Id\" IS NOT NULL");

            // Add shadow property for singleton enforcement
            builder.Property<int>("SingletonKey")
                   .HasDefaultValue(1)
                   .IsRequired();

            // Unique index ensures only one row can exist
            builder.HasIndex("SingletonKey").IsUnique();

        }
    }
}
