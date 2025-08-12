using IgniteLifeApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IgniteLifeApi.Infrastructure.Data.Configurations
{
    public class BookingRuleBlockedPeriodConfiguration : IEntityTypeConfiguration<BookingRuleBlockedPeriod>
    {
        public void Configure(EntityTypeBuilder<BookingRuleBlockedPeriod> builder)
        {
        }
    }
}
