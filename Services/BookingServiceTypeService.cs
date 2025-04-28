using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Dtos;
using Server.Models;


namespace Server.Services;

public class BookingServiceTypeService(ApplicationDbContext Context)
{
    private readonly ApplicationDbContext _context = Context;

    public async Task<ApiResult<IEnumerable<BookingServiceType>>> GetAllBookingServiceAsync()
    {
        var result = await _context.BookingServiceType.ToListAsync();
        return ApiResult<IEnumerable<BookingServiceType>>.Success(result);
    }

    public async Task<ApiResult<IEnumerable<BookingServiceType>>> PostBookingServiceAsync(CreateBookingServiceTypeRequest request)
    {

        var bookingService = new BookingServiceType
        {
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            Duration = request.Duration
        };
        _context.BookingServiceType.Add(bookingService);
        await _context.SaveChangesAsync();
        var result = await GetAllBookingServiceAsync();
        return result;
    }

    public async Task<ApiResult<IEnumerable<BookingServiceType>>> PatchBookingServiceAsync(Guid id, UpdateBookingServiceTypeRequest request)
    {
        var bookingService = await _context.BookingServiceType.FindAsync(id);

        if (bookingService == null)
        {
            return ApiResult<IEnumerable<BookingServiceType>>.Fail("Booking service not found.");
        }

        // Update only the fields provided in the request
        if (request.Name != null) bookingService.Name = request.Name;
        if (request.Description != null) bookingService.Description = request.Description;
        if (request.Price.HasValue) bookingService.Price = request.Price.Value;
        if (request.Duration.HasValue) bookingService.Duration = request.Duration.Value;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            return ApiResult<IEnumerable<BookingServiceType>>.Fail("A concurrency error occurred while updating.");
        }

        var result = await GetAllBookingServiceAsync();
        return result;
    }


    public async Task<ApiResult<IEnumerable<BookingServiceType>>> DeleteBookingServiceAsync(Guid id)
    {
        var bookingService = await _context.BookingServiceType.FindAsync(id);
        if (bookingService == null)
        {
            return ApiResult<IEnumerable<BookingServiceType>>.Fail("Booking service not found.");
        }

        _context.BookingServiceType.Remove(bookingService);
        await _context.SaveChangesAsync();

        var result = await GetAllBookingServiceAsync();
        return result;
    }
}