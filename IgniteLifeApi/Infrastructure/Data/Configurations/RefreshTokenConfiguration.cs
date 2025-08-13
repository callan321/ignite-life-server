using IgniteLifeApi.Domain.Constants;
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
               .HasMaxLength(FieldLengths.EncodedHash);

        builder.Property(rt => rt.ReplacedByTokenHash)
               .HasMaxLength(FieldLengths.EncodedHash);

        builder.Property(rt => rt.ExpiresAtUtc)
               .IsRequired()
               .HasColumnType("timestamptz");

        builder.Property(rt => rt.RevokedAtUtc)
               .HasColumnType("timestamptz");

        builder.Property(rt => rt.CreatedAtUtc).HasColumnType("timestamptz");
        builder.Property(rt => rt.UpdatedAtUtc).HasColumnType("timestamptz");

        // Indexes
        builder.HasIndex(rt => rt.TokenHash).IsUnique();
        builder.HasIndex(rt => rt.UserId);
        builder.HasIndex(rt => rt.ExpiresAtUtc);
    }
}
