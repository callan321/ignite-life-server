using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Dtos;
using Server.Models;


namespace Server.Services;

public class BookingServicesService(ApplicationDbContext Context)
{
    private readonly ApplicationDbContext _context = Context;

    public async Task<ApiResult<IEnumerable<BookingService>>> GetAllBookingServiceAsync()
    {
        var result = await _context.BookingServices.ToListAsync();
        return ApiResult<IEnumerable<BookingService>>.Success(result);
    }

    public async Task<ApiResult<IEnumerable<BookingService>>> PostBookingServiceAsync(CreateBookingServiceRequest request)
    {

        var bookingService = new BookingService
        {
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            Duration = request.Duration
        };
        _context.BookingServices.Add(bookingService);
        await _context.SaveChangesAsync();
        var result = await GetAllBookingServiceAsync();
        return result;
    }

    public async Task<ApiResult<IEnumerable<BookingService>>> PatchBookingServiceAsync(Guid id, UpdateBookingServiceRequest request)
    {
        var bookingService = await _context.BookingServices.FindAsync(id);

        if (bookingService == null)
        {
            return ApiResult<IEnumerable<BookingService>>.Fail("Booking service not found.");
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
            return ApiResult<IEnumerable<BookingService>>.Fail("A concurrency error occurred while updating.");
        }

        var result = await GetAllBookingServiceAsync();
        return result;
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