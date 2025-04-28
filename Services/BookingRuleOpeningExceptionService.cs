using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Dtos;
using Server.Models;
using Server.Utils;

namespace Server.Services;

public class BookingRuleOpeningExceptionService(ApplicationDbContext dbContext)
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    public async Task<ApiResult<ReadBookingRulesResponse>> CreateOpeningExceptionAsync(CreateBookingRuleOpeningExceptionRequest request)
    {
        if (!DateTimeUtils.IsValidRange(request.StartTime, request.EndTime))
            return ApiResult<ReadBookingRulesResponse>.Fail("Start time must be before end time.");

        var defaultRules = await BookingRulesUtils.GetDefaultBookingRules(_dbContext);
        if (defaultRules == null)
            return ApiResult<ReadBookingRulesResponse>.Fail("Default booking rules not found.");

        var exceptions = await _dbContext.BookingRuleOpeningExceptions
            .Where(e => e.BookingRuleId == defaultRules.Id)
            .ToListAsync();

        bool overlaps = exceptions.Any(e =>
            DateTimeUtils.IsOverlapping(request.StartTime, request.EndTime, e.StartTime, e.EndTime));

        if (overlaps)
            return ApiResult<ReadBookingRulesResponse>.Fail("Overlapping opening exception exists.");

        var newException = new BookingRuleOpeningException
        {
            BookingRuleId = defaultRules.Id,
            StartTime = request.StartTime,
            EndTime = request.EndTime
        };

        _dbContext.BookingRuleOpeningExceptions.Add(newException);
        await _dbContext.SaveChangesAsync();

        return await GetRefreshedBookingRulesResultAsync();
    }

    public async Task<ApiResult<ReadBookingRulesResponse>> UpdateOpeningExceptionAsync(Guid id, UpdateBookingRuleOpeningExceptionRequest request)
    {
        var exception = await _dbContext.BookingRuleOpeningExceptions.FindAsync(id);
        if (exception == null)
            return ApiResult<ReadBookingRulesResponse>.Fail("Exception not found.");

        var proposedStart = request.StartTime ?? exception.StartTime;
        var proposedEnd = request.EndTime ?? exception.EndTime;

        if (!DateTimeUtils.IsValidRange(proposedStart, proposedEnd))
            return ApiResult<ReadBookingRulesResponse>.Fail("Start time must be before end time.");

        exception.StartTime = proposedStart;
        exception.EndTime = proposedEnd;

        await _dbContext.SaveChangesAsync();

        return await GetRefreshedBookingRulesResultAsync();
    }


    public async Task<ApiResult<ReadBookingRulesResponse>> DeleteOpeningExceptionAsync(Guid id)
    {
        var exception = await _dbContext.BookingRuleOpeningExceptions.FindAsync(id);
        if (exception == null)
            return ApiResult<ReadBookingRulesResponse>.Fail("Exception not found.");

        _dbContext.BookingRuleOpeningExceptions.Remove(exception);
        await _dbContext.SaveChangesAsync();

        return await GetRefreshedBookingRulesResultAsync();
    }

    private async Task<ApiResult<ReadBookingRulesResponse>> GetRefreshedBookingRulesResultAsync()
    {
        var updated = await BookingRulesUtils.GetDefaultBookingRules(_dbContext);
        return updated is null
            ? ApiResult<ReadBookingRulesResponse>.Fail("Failed to retrieve updated booking rules.")
            : ApiResult<ReadBookingRulesResponse>.Success(BookingRulesUtils.CreateReadBookingRulesResponse(updated));
    }
}
