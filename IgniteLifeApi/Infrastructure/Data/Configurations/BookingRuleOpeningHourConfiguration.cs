using IgniteLifeApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IgniteLifeApi.Infrastructure.Data.Configurations
{
    public class BookingRuleOpeningHourConfiguration : IEntityTypeConfiguration<BookingRuleOpeningHour>
    {
        public void Configure(EntityTypeBuilder<BookingRuleOpeningHour> builder)
        {
            builder.ToTable("booking_rule_opening_hours");

            builder.HasKey(h => h.Id);

            builder.Property(h => h.DayOfWeek)
                   .IsRequired();

            builder.Property(h => h.OpenTimeUtc)
                   .IsRequired();

            builder.Property(h => h.CloseTimeUtc)
                   .IsRequired();

            builder.Property(h => h.IsClosed)
                   .IsRequired();

            builder.Property(h => h.BookingRulesId)
                   .IsRequired();

            builder.HasIndex(h => new { h.DayOfWeek, h.BookingRulesId })
                   .IsUnique();

            builder.HasOne(h => h.BookingRules)
                   .WithMany(r => r.OpeningHours)
                   .HasForeignKey(h => h.BookingRulesId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
