using IgniteLifeApi.Domain.Entities;
using IgniteLifeApi.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace IgniteLifeApi.Infrastructure.Data
{
    public class ApplicationDbContext
        : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<BookingRuleOpeningHour> BookingRuleOpeningHours { get; set; } = null!;
        public DbSet<BookingRuleBlockedPeriod> BookingRuleBlockedPeriods { get; set; } = null!;
        public DbSet<BookingRules> BookingRules { get; set; } = null!;
        public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;

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
            var now = DateTime.UtcNow;

            foreach (var entry in ChangeTracker.Entries<IHasTimestamps>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        if (entry.Entity.CreatedAtUtc == default)
                            entry.Entity.CreatedAtUtc = now;
                        entry.Entity.UpdatedAtUtc = now;
                        break;

                    case EntityState.Modified:
                        entry.Property(e => e.CreatedAtUtc).IsModified = false;
                        entry.Entity.UpdatedAtUtc = now;
                        break;
                }
            }
        }
    }
}
