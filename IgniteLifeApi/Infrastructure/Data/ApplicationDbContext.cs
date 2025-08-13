using IgniteLifeApi.Domain.Entities;
using IgniteLifeApi.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace IgniteLifeApi.Infrastructure.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
    {
        public DbSet<Booking> Bookings { get; set; } = null!;
        public DbSet<BookingRuleOpeningHour> BookingRuleOpeningHours { get; set; } = null!;
        public DbSet<BookingRuleBlockedPeriod> BookingRuleBlockedPeriods { get; set; } = null!;
        public DbSet<BookingRules> BookingRules { get; set; } = null!;
        public DbSet<BookingServiceType> BookingServiceTypes { get; set; } = null!;
        public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
        public DbSet<ApplicationUser> AdminUsers { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
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
            var entries = ChangeTracker.Entries<IHasTimestamps>()
                .Where(e => e.State is EntityState.Added or EntityState.Modified);

            var now = DateTime.UtcNow;
            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                    entry.Entity.CreatedAt = now;
                entry.Entity.UpdatedAt = now;
            }
        }
    }
}