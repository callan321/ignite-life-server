using IgniteLifeApi.Models;
using Microsoft.EntityFrameworkCore;
using Server.Models;

namespace IgniteLifeApi.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<UserProfile> UserProfiles { get; set; } = null!;
    public DbSet<UserInfo> UserInfos { get; set; } = null!;
    public DbSet<Booking> Bookings { get; set; } = null!;
    public DbSet<BookingRuleOpeningHour> BookingRuleOpeningHours { get; set; } = null!;
    public DbSet<BookingRuleBlockedPeriod> BookingRuleBlockedPeriod { get; set; } = null!;
    public DbSet<BookingRules> BookingRules { get; set; } = null!;
    public DbSet<BookingServiceType> BookingServiceType { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure the one-to-one relationship between UserProfile and UserInfo
        modelBuilder.Entity<UserProfile>()
            .HasOne(up => up.Info)
            .WithOne(ui => ui.UserProfile)
            .HasForeignKey<UserInfo>(ui => ui.UserProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure the one-to-many relationship between UserProfile and Booking
        modelBuilder.Entity<UserProfile>()
            .HasMany(u => u.Bookings)
            .WithOne(b => b.User)
            .HasForeignKey(b => b.UserProfileId)
            .OnDelete(DeleteBehavior.SetNull);

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

        modelBuilder.Entity<BookingServiceType>()
        .HasIndex(s => s.Name)
        .IsUnique();

    }

    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries<BaseEntity>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
                entry.Entity.CreatedAt = DateTime.UtcNow;

            entry.Entity.UpdatedAt = DateTime.UtcNow;
        }
    }
}
