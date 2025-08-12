using IgniteLifeApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IgniteLifeApi.Infrastructure.Data.Configurations;

public class BookingServiceTypeConfiguration : IEntityTypeConfiguration<BookingServiceType>
{
    public void Configure(EntityTypeBuilder<BookingServiceType> builder)
    {
        builder.HasIndex(s => s.Name).IsUnique();
    }
}
