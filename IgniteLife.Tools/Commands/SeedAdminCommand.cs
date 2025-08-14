using IgniteLife.Tools.Commands.Common;
using IgniteLifeApi.Domain.Entities;
using IgniteLifeApi.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace IgniteLife.Tools.Commands
{
    public sealed class SeedAdminCommand : ICommand<SeedAdminCommand>
    {
        public static string Name => "seed-admin";

        public static void WriteUsage()
            => Console.WriteLine("Usage: seed-admin <email> <password> [EnvironmentName=Production]");

        public static async Task RunAsync(string[] args)
        {
            if (args.Length < 2) { WriteUsage(); return; }

            var email = args[0];
            var password = args[1];
            var envName = (args.Length >= 3 && !string.IsNullOrWhiteSpace(args[2]))
                ? args[2]
                : Environments.Production; // default to PROD

            using var host = HostFactory.Create(envName, includeIdentity: true);
            using var scope = host.Services.CreateScope();
            var sp = scope.ServiceProvider;

            var db = sp.GetRequiredService<ApplicationDbContext>();
            var users = sp.GetRequiredService<UserManager<ApplicationUser>>();
            var roles = sp.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

            await db.Database.MigrateAsync();

            const string AdminRole = "Admin";

            // Ensure role
            if (!await roles.RoleExistsAsync(AdminRole))
            {
                var roleRes = await roles.CreateAsync(new IdentityRole<Guid>
                {
                    Name = AdminRole,
                    NormalizedName = AdminRole.ToUpperInvariant()
                });
                if (!roleRes.Succeeded)
                {
                    Console.WriteLine("Failed to create role:");
                    foreach (var e in roleRes.Errors) Console.WriteLine($" - {e.Code}: {e.Description}");
                    return;
                }
            }

            // Ensure user
            var user = await users.FindByEmailAsync(email);
            if (user is null)
            {
                user = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true };
                var create = await users.CreateAsync(user, password);
                if (!create.Succeeded)
                {
                    Console.WriteLine("Failed to create user:");
                    foreach (var e in create.Errors) Console.WriteLine($" - {e.Code}: {e.Description}");
                    return;
                }
                Console.WriteLine($"Created user {email}");
            }
            else
            {
                Console.WriteLine($"User {email} already exists");
            }

            // Ensure membership
            if (!await users.IsInRoleAsync(user, AdminRole))
            {
                var add = await users.AddToRoleAsync(user, AdminRole);
                if (!add.Succeeded)
                {
                    Console.WriteLine("Failed to add user to role:");
                    foreach (var e in add.Errors) Console.WriteLine($" - {e.Code}: {e.Description}");
                    return;
                }
                Console.WriteLine($"Added {email} to role {AdminRole}");
            }
            else
            {
                Console.WriteLine($"{email} already in role {AdminRole}");
            }

            Console.WriteLine($"Done. (Environment: {envName})");
        }
    }
}
