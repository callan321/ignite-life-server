using IgniteLifeApi.Domain.Entities;
using IgniteLifeApi.Domain.Models.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IgniteLifeApi.Infrastructure.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Booking> Bookings { get; set; } = null!;
    public DbSet<BookingRuleOpeningHour> BookingRuleOpeningHours { get; set; } = null!;
    public DbSet<BookingRuleBlockedPeriod> BookingRuleBlockedPeriod { get; set; } = null!;
    public DbSet<BookingRules> BookingRules { get; set; } = null!;
    public DbSet<BookingServiceType> BookingServiceType { get; set; } = null!;
    public DbSet<RefreshToken> RefreshToken { get; set; } = null!;
    public DbSet<AdminUser> AdminUsers { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure the one-to-many relationship between AdminUser and RefreshToken
        modelBuilder.Entity<AdminUser>()
            .HasMany(a => a.RefreshTokens)
            .WithOne(rt => rt.User)
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure the one-to-many relationship between Booking and BookingService
        modelBuilder.Entity<Booking>()
            .HasOne(b => b.Service)
            .WithMany()
            .HasForeignKey(b => b.ServiceId)
            .OnDelete(DeleteBehavior.SetNull);

        // Configure booking rules to have isDefault property indexed
        modelBuilder.Entity<BookingRules>()
            .HasIndex(br => br.IsDefault)
            .IsUnique()
            .HasFilter("\"IsDefault\" = TRUE");

        // Configure the one-to-many relationship between BookingRules and OpeningHours
        modelBuilder.Entity<BookingRules>()
            .HasMany(r => r.OpeningHours)
            .WithOne()
            .HasForeignKey(h => h.BookingRulesId)
            .OnDelete(DeleteBehavior.Cascade);

        // Constraint to ensure that the same day of the week cannot have multiple opening hours for the same BookingRules
        modelBuilder.Entity<BookingRuleOpeningHour>()
            .HasIndex(h => new { h.DayOfWeek, h.BookingRulesId })
            .IsUnique();

        // Configure the one-to-many relationship between BookingRules and BlockedPeriods
        modelBuilder.Entity<BookingServiceType>()
        .HasIndex(s => s.Name)
        .IsUnique();

    }

    // SaveChanges override to ensure timestamps are updated
    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    // SaveChangesAsync override to ensure timestamps are updated asynchronously
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    // Update timestamps for entities implementing IHasTimestamps
    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries<IHasTimestamps>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
                entry.Entity.CreatedAt = DateTime.UtcNow;

            entry.Entity.UpdatedAt = DateTime.UtcNow;
        }
    }
}
