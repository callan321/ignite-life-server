using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Models;

namespace Server.Services;

public class BookingTimeSlotService(ApplicationDbContext context, BookingRulesService bookingRulesService)
{
    private readonly ApplicationDbContext _context = context;
    private readonly BookingRulesService _bookingRulesService = bookingRulesService;

    public async Task<List<BookingTimeSlot>> GetAvailableTimeSlotsAsync(DateTime date)
    {
        var bookings = await _context.Bookings
            .Where(b => b.StartTime.Date == date.Date)
            .ToListAsync();
        var timeSlots = new List<BookingTimeSlot>();
        return timeSlots;
    }

    public async void GenerateTimeSlotsAsync()
    {
        var rules = await _bookingRulesService.GetDefaultBookingRulesAsync();
        if (!rules.IsSuccess)
            throw new Exception("Default booking rules not found.");

        var bookingRules = rules.Data;

        foreach (var openingHour in bookingRules.OpeningHours)
        {
            // openingHour.DayOfWeek (Monday, Tuesday, etc.)
            // openingHour.OpenTime (e.g., 09:00 AM)
            // openingHour.CloseTime (e.g., 05:00 PM)

            var currentStart = openingHour.OpenTime;
            var closeTime = openingHour.CloseTime;





        }
    }
}
