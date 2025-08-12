using IgniteLifeApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IgniteLifeApi.Infrastructure.Data.Configurations;

public class BookingRuleOpeningHourConfiguration : IEntityTypeConfiguration<BookingRuleOpeningHour>
{
    public void Configure(EntityTypeBuilder<BookingRuleOpeningHour> builder)
    {
        builder.HasIndex(h => new { h.DayOfWeek, h.BookingRulesId })
               .IsUnique();
    }
}
