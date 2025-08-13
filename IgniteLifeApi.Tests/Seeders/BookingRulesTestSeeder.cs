using IgniteLifeApi.Domain.Entities;
using IgniteLifeApi.Infrastructure.Data;

namespace IgniteLifeApi.Tests.Seeders
{
    public static class BookingRulesTestSeeder
    {
        public static readonly Guid DefaultBookingRulesId = Guid.Parse("11111111-2222-3333-4444-555555555555");

        /// <summary>
        /// Seeds a BookingRules singleton into the provided context (Postgres, InMemory, etc.).
        /// Always wipes existing BookingRules and related entities first.
        /// Seeds full week opening hours by default (9–5 each day).
        /// </summary>
        public static BookingRules SeedBookingRules(
            ApplicationDbContext context,
            Action<BookingRules>? configure = null)
        {
            // Ensure DB is clean so that fixed GUIDs don't conflict
            context.BookingRules.RemoveRange(context.BookingRules);
            context.BookingRuleOpeningHours.RemoveRange(context.BookingRuleOpeningHours);
            context.BookingRuleBlockedPeriods.RemoveRange(context.BookingRuleBlockedPeriods);
            context.SaveChanges();

            var rules = new BookingRules
            {
                Id = DefaultBookingRulesId,
                MaxAdvanceBookingDays = 10,
                MinAdvanceBookingHours = 2,
                BufferBetweenBookingsMinutes = 30,
                SlotDurationMinutes = 60,
                OpeningHours = [],
                BlockedPeriods = []
            };

            // Seed default opening hours for all 7 days
            foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
            {
                rules.OpeningHours.Add(new BookingRuleOpeningHour
                {
                    Id = Guid.NewGuid(), // new GUID for each day
                    BookingRulesId = rules.Id,
                    DayOfWeek = day,
                    OpenTimeUtc = new TimeOnly(9, 0),
                    CloseTimeUtc = new TimeOnly(17, 0),
                    IsClosed = false
                });
            }

            // Allow custom overrides in tests
            configure?.Invoke(rules);

            context.BookingRules.Add(rules);
            context.SaveChanges();
            return rules;
        }

        /// <summary>
        /// Gets the GUID for the seeded BookingRules entity.
        /// </summary>
        public static Guid GetBookingRuleGuid() => DefaultBookingRulesId;

        /// <summary>
        /// Retrieves the seeded opening hour for a specific day of the week.
        /// </summary>
        public static BookingRuleOpeningHour? GetOpeningHour(ApplicationDbContext context, DayOfWeek day)
        {
            return context.BookingRuleOpeningHours
                .FirstOrDefault(oh => oh.BookingRulesId == DefaultBookingRulesId && oh.DayOfWeek == day);
        }

        /// <summary>
        /// Retrieves all seeded opening hours for the default BookingRules.
        /// </summary>
        public static List<BookingRuleOpeningHour> GetOpeningHours(ApplicationDbContext context)
        {
            return context.BookingRuleOpeningHours
                .Where(oh => oh.BookingRulesId == DefaultBookingRulesId)
                .OrderBy(oh => oh.DayOfWeek)
                .ToList();
        }
    }
}
