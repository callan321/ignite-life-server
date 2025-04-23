using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Dtos;
using Server.Models;
using System.Collections.Generic;


namespace Server.Services;

public class BookingServicesService(ApplicationDbContext Context)
{
    private readonly ApplicationDbContext _context = Context;

    public async Task<ApiResult<IEnumerable<BookingService>>> GetAllBookingServiceAsync()
    {
        var result = await _context.BookingServices.ToListAsync();
        return ApiResult<IEnumerable<BookingService>>.Success(result);
    }

    public async Task<ApiResult<IEnumerable<BookingService>>> DeleteBookingServiceAsync(Guid id)
    {
        var bookingService = await _context.BookingServices.FindAsync(id);
        if (bookingService == null)
        {
            return ApiResult<IEnumerable<BookingService>>.Fail("Booking service not found.");
        }

        _context.BookingServices.Remove(bookingService);
        await _context.SaveChangesAsync();

        var result = await GetAllBookingServiceAsync();
        return result;
    }



}