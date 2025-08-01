using IgniteLifeApi.Data;
using Microsoft.EntityFrameworkCore;
using Server.Dtos;
using Server.Models;

namespace Server.Utils;

public static class BookingRulesUtils
{
    // Static helper to use from other services
    public static async Task<BookingRules?> GetDefaultBookingRules(ApplicationDbContext context)
    {
        var bookingRules = await context.BookingRules
            .Where(b => b.IsDefault)
            .Include(b => b.OpeningHours)
            .Include(b => b.OpeningExceptions.Where(e => e.EndTime >= DateTime.UtcNow))
            .FirstOrDefaultAsync();

        if (bookingRules != null)
            bookingRules.OpeningHours = [.. bookingRules.OpeningHours.OrderBy(h => h.DayOfWeek)];

        return bookingRules;
    }

    // Static DTO mapper
    public static ReadBookingRulesResponse CreateReadBookingRulesResponse(BookingRules bookingRules)
    {
        return new ReadBookingRulesResponse
        {
            MaxAdvanceBookingDays = bookingRules.MaxAdvanceBookingDays,
            BufferBetweenBookingsMinutes = bookingRules.BufferBetweenBookingsMinutes,
            SlotDurationMinutes = bookingRules.SlotDurationMinutes,
            MinAdvanceBookingHours = bookingRules.MinAdvanceBookingHours,
            OpeningHours = [.. bookingRules.OpeningHours.Select(h => new ReadBookingRuleOpeningHourResponse
        {
            DayOfWeek = h.DayOfWeek,
            OpenTime = h.OpenTime,
            CloseTime = h.CloseTime
        })],
            OpeningExceptions = [.. bookingRules.OpeningExceptions.Select(e => new ReadBookingRuleOpeningExceptionResponse
        {
            Id = e.Id,
            StartTime = e.StartTime,
            EndTime = e.EndTime
        })]
        };
    }

}
