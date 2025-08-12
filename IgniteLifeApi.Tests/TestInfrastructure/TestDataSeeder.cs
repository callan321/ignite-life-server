using IgniteLifeApi.Domain.Entities;
using IgniteLifeApi.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace IgniteLifeApi.Tests.TestInfrastructure
{
    public static class TestDataSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AdminUser>>();

            // Reset database
            await db.Database.EnsureDeletedAsync();
            await db.Database.MigrateAsync();

            // --- Seed Admin User ---
            var adminEmail = "admin@example.com";
            var adminPassword = "Admin!23456789";

            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var admin = new AdminUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    LockoutEnabled = true
                };

                var result = await userManager.CreateAsync(admin, adminPassword);
                if (!result.Succeeded)
                    throw new Exception("Failed to seed admin: " + string.Join(", ", result.Errors.Select(e => e.Description)));
            }

            // --- Seed domain data ---
            await db.SaveChangesAsync();
        }
    }
}
