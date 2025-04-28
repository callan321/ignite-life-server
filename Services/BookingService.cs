using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Models;

namespace Server.Services;

public class BookingService(ApplicationDbContext context)
{
    private readonly ApplicationDbContext _context = context;

    public async Task<List<Booking>> GetAvailableBookingsAsync()
    {
        return await _context.Bookings.ToListAsync();
    }

}
