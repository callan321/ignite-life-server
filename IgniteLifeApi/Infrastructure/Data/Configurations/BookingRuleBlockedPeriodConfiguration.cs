using IgniteLifeApi.Domain.Constants;
using IgniteLifeApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IgniteLifeApi.Infrastructure.Data.Configurations
{
    public class BookingRuleBlockedPeriodConfiguration : IEntityTypeConfiguration<BookingRuleBlockedPeriod>
    {
        public void Configure(EntityTypeBuilder<BookingRuleBlockedPeriod> builder)
        {
            builder.ToTable("booking_rule_blocked_periods");

            builder.HasKey(b => b.Id);

            builder.Property(b => b.StartDateTimeUtc)
                   .IsRequired()
                   .HasColumnType("timestamptz");

            builder.Property(b => b.EndDateTimeUtc)
                   .IsRequired()
                   .HasColumnType("timestamptz");

            builder.Property(b => b.Description)
                   .HasMaxLength(FieldLengths.ShortText);

            builder.Property(b => b.CreatedAtUtc).HasColumnType("timestamptz");
            builder.Property(b => b.UpdatedAtUtc).HasColumnType("timestamptz");

            builder.Property(b => b.BookingRulesId)
                   .IsRequired();

            builder.HasOne(b => b.BookingRules)
                   .WithMany(r => r.BlockedPeriods)
                   .HasForeignKey(b => b.BookingRulesId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
