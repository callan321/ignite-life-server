using IgniteLife.Tools.Commands.Common;
using IgniteLifeApi.Domain.Entities;
using IgniteLifeApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace IgniteLife.Tools.Commands
{
    public sealed class SeedBookingRulesCommand : ICommand<SeedBookingRulesCommand>
    {
        public static string Name => "seed-booking-rules";

        // Args: [EnvironmentName]
        public static void WriteUsage()
            => Console.WriteLine("Usage: seed-booking-rules [EnvironmentName=Production]");

        public static async Task RunAsync(string[] args)
        {
            var envName = (args.Length >= 1 && !string.IsNullOrWhiteSpace(args[0]))
                ? args[0]
                : Environments.Production; // default to PROD

            using var host = HostFactory.Create(envName, includeIdentity: false);
            using var scope = host.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            await db.Database.MigrateAsync();

            if (await db.BookingRules.AnyAsync())
            {
                Console.WriteLine("Booking rules already exist. Nothing to do.");
                Console.WriteLine($"Done. (Environment: {envName})");
                return;
            }

            // Base rules row
            var rules = new BookingRules();
            db.BookingRules.Add(rules);
            await db.SaveChangesAsync();

            // Opening hours for every day (09:00–17:00)
            var days = new[]
            {
                DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday,
                DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday
            };

            foreach (var dow in days)
            {
                db.BookingRuleOpeningHours.Add(new BookingRuleOpeningHour
                {
                    BookingRulesId = rules.Id,
                    DayOfWeek = dow,
                    OpenTimeUtc = new TimeOnly(9, 0),
                    CloseTimeUtc = new TimeOnly(17, 0)
                });
            }

            // Optional sample blocked period next week
            db.BookingRuleBlockedPeriods.Add(new BookingRuleBlockedPeriod
            {
                BookingRulesId = rules.Id,
                StartDateTimeUtc = DateTime.UtcNow.Date.AddDays(7),
                EndDateTimeUtc = DateTime.UtcNow.Date.AddDays(8),
                Description = "Clinic closed (sample seed)"
            });

            await db.SaveChangesAsync();

            Console.WriteLine("Seeded booking rules with opening hours for all days.");
            Console.WriteLine($"Done. (Environment: {envName})");
        }
    }
}
