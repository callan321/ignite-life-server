using Server.Models;

namespace Server.Data;

public static class DbSeeder
{
    public static void Seed(ApplicationDbContext db)
    {
        if (!db.BookingRules.Any())
        {
            var today = DateTime.UtcNow.Date;
            var nextWednesday = today.AddDays(((int)DayOfWeek.Wednesday - (int)today.DayOfWeek + 7) % 7 + 7);
            var followingThursday = nextWednesday.AddDays(8);

            var rules = new BookingRules
            {
                BufferBetweenBookingsMinutes = 30,
                IsDefault = true,

                OpeningHours =
                [
                new() { DayOfWeek = DayOfWeek.Monday, OpenTime = new(9, 0), CloseTime = new(17, 0) },
                new() { DayOfWeek = DayOfWeek.Tuesday, OpenTime = new(9, 0), CloseTime = new(17, 0) },
                new() { DayOfWeek = DayOfWeek.Wednesday, OpenTime = new(9, 0), CloseTime = new(17, 0) },
                new() { DayOfWeek = DayOfWeek.Thursday, OpenTime = new(9, 0), CloseTime = new(17, 0) },
                new() { DayOfWeek = DayOfWeek.Friday, OpenTime = new(9, 0), CloseTime = new(17, 0) },
                new() { DayOfWeek = DayOfWeek.Saturday, OpenTime = new(10, 0), CloseTime = new(14, 0) },
                new() { DayOfWeek = DayOfWeek.Sunday, OpenTime = new(0, 0), CloseTime = new(0, 0) },
            ],

                OpeningExceptions =
                [
                    new BookingRuleOpeningException
                {
                    StartTime = nextWednesday.AddHours(10),
                    EndTime = nextWednesday.AddHours(12)
                },
                new BookingRuleOpeningException
                {
                    StartTime = followingThursday.AddHours(9),
                    EndTime = followingThursday.AddHours(17)
                }
                ]
            };

            db.BookingRules.Add(rules);
        }

        if (!db.BookingServices.Any())
        {
            db.BookingServices.AddRange(
                new BookingService
                {
                    Name = "Massage",
                    Description = "Relaxing full-body massage",
                    Price = 80m,
                    Duration = "01:00"
                },
                new BookingService
                {
                    Name = "Yoga Session",
                    Description = "Group yoga session with instructor",
                    Price = 50m,
                    Duration = "00:45"
                },
                new BookingService
                {
                    Name = "Nutrition Consultation",
                    Description = "1-on-1 with a registered dietitian",
                    Price = 120m,
                    Duration = "01:15"
                }
            );
        }

        db.SaveChanges();
    }

}
