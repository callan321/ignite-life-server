using IgniteLifeApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IgniteLifeApi.Infrastructure.Data.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");

        builder.HasKey(rt => rt.Id);

        builder.HasOne(rt => rt.User)
               .WithMany(u => u.RefreshTokens)
               .HasForeignKey(rt => rt.UserId)
               .OnDelete(DeleteBehavior.Cascade);

        // Required/lengths/types
        builder.Property(rt => rt.TokenHash)
               .IsRequired()
               .HasMaxLength(64);

        builder.Property(rt => rt.ReplacedByTokenHash)
               .HasMaxLength(64);

        builder.Property(rt => rt.IpAddress)
               .HasMaxLength(45);

        builder.Property(rt => rt.UserAgent)
               .HasMaxLength(512);

        builder.Property(rt => rt.ExpiresAtUtc)
               .IsRequired()
               .HasColumnType("timestamptz");

        builder.Property(rt => rt.RevokedAtUtc)
               .HasColumnType("timestamptz");

        builder.Property(rt => rt.CreatedAt).HasColumnType("timestamptz");
        builder.Property(rt => rt.UpdatedAt).HasColumnType("timestamptz");

        // Indexes
        builder.HasIndex(rt => rt.TokenHash).IsUnique();
        builder.HasIndex(rt => rt.UserId);
        builder.HasIndex(rt => rt.ExpiresAtUtc);

        // Helpful filtered indexes (Postgres)
        builder.HasIndex(rt => new { rt.UserId, rt.ExpiresAtUtc })
               .HasFilter("\"RevokedAtUtc\" IS NULL");

        builder.HasIndex(rt => new { rt.TokenHash, rt.RevokedAtUtc })
               .HasFilter("\"RevokedAtUtc\" IS NULL");
    }
}
