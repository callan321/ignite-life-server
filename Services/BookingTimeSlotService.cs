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
            // Skip if the opening and closing times are the same (closed day like Sunday maybe)
            if (openingHour.OpenTime == openingHour.CloseTime)
                continue;

            var currentStart = openingHour.OpenTime;
            var closeTime = openingHour.CloseTime;
            var slotDuration = TimeSpan.FromMinutes(bookingRules.SlotDurationMinutes);

            while (currentStart + slotDuration <= closeTime)
            {
                var slot = new BookingTimeSlot
                {
                    StartTime = currentStart,
                    EndTime = currentStart + slotDuration,
                    IsAvailable = true
                };

                timeSlots.Add(slot);

                currentStart += slotDuration; // Move to the next slot
            }
        }

    }
}
