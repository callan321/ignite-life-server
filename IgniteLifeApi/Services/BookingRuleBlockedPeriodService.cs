using IgniteLifeApi.Data;
using IgniteLifeApi.Dtos.BookingRuleBlockedPeriod;
using IgniteLifeApi.Models;
using IgniteLifeApi.Services.Common;
using Microsoft.EntityFrameworkCore;

namespace IgniteLifeApi.Services;

public class BookingRuleBlockedPeriodService(ApplicationDbContext dbContext)
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    public async Task<ServiceResult<List<BookingRuleBlockedPeriodDto>>> GetAllBookingRuleBlockedPeriodsAsync()
    {
        var blockedPeriods = await _dbContext.BookingRuleBlockedPeriod
            .OrderBy(b => b.StartDateTime)
            .ToListAsync();

        var dtos = blockedPeriods.Select(b => new BookingRuleBlockedPeriodDto
        {
            Id = b.Id,
            StartDateTime = b.StartDateTime,
            EndDateTime = b.EndDateTime,
            Description = b.Description
        }).ToList();

        return ServiceResult<List<BookingRuleBlockedPeriodDto>>.SuccessResult(dtos);
    }

    public async Task<ServiceResult<BookingRuleBlockedPeriodDto>> CreateBlockedPeriodAsync(CreateBookingRuleBlockedPeriodDto dto)
    {
        if (dto == null)
            return ServiceResult<BookingRuleBlockedPeriodDto>.BadRequest("Request body cannot be null.");

        if (dto.StartDateTime >= dto.EndDateTime)
            return ServiceResult<BookingRuleBlockedPeriodDto>.BadRequest("StartTime must be before EndTime.");

        var isOverlap = await _dbContext.BookingRuleBlockedPeriod.AnyAsync(b =>
            b.StartDateTime < dto.EndDateTime && dto.StartDateTime < b.EndDateTime);

        if (isOverlap)
            return ServiceResult<BookingRuleBlockedPeriodDto>.Conflict("The blocked period overlaps with an existing blocked period.");

        if (!dto.StartDateTime.HasValue || !dto.EndDateTime.HasValue)
            return ServiceResult<BookingRuleBlockedPeriodDto>.BadRequest("StartDateTime and EndDateTime are required.");

        var entity = new BookingRuleBlockedPeriod
        {
            Id = Guid.NewGuid(),
            StartDateTime = (DateTime)dto.StartDateTime,
            EndDateTime = (DateTime)dto.EndDateTime,
            Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description
        };

        _dbContext.BookingRuleBlockedPeriod.Add(entity);
        await _dbContext.SaveChangesAsync();

        var resultDto = new BookingRuleBlockedPeriodDto
        {
            Id = entity.Id,
            StartDateTime = entity.StartDateTime,
            EndDateTime = entity.EndDateTime,
            Description = entity.Description
        };

        return ServiceResult<BookingRuleBlockedPeriodDto>.CreatedResult(resultDto, "Blocked period created.");
    }

    public async Task<ServiceResult<BookingRuleBlockedPeriodDto>> UpdateBlockedPeriodAsync(UpdateBookingRuleBlockedPeriodDto dto)
    {
        if (dto == null)
            return ServiceResult<BookingRuleBlockedPeriodDto>.BadRequest("Request body cannot be null.");

        var entity = await _dbContext.BookingRuleBlockedPeriod.FindAsync(dto.Id);
        if (entity == null)
            return ServiceResult<BookingRuleBlockedPeriodDto>.NotFound("Blocked period not found.");

        if (!dto.StartDateTime.HasValue && !dto.EndDateTime.HasValue && string.IsNullOrWhiteSpace(dto.Description))
            return ServiceResult<BookingRuleBlockedPeriodDto>.BadRequest("At least one field must be provided for update.");

        var newStart = dto.StartDateTime ?? entity.StartDateTime;
        var newEnd = dto.EndDateTime ?? entity.EndDateTime;

        if (newStart >= newEnd)
            return ServiceResult<BookingRuleBlockedPeriodDto>.BadRequest("StartTime must be before EndTime.");

        var isOverlap = await _dbContext.BookingRuleBlockedPeriod.AnyAsync(b =>
            b.Id != dto.Id && b.StartDateTime < newEnd && newStart < b.EndDateTime);

        if (isOverlap)
            return ServiceResult<BookingRuleBlockedPeriodDto>.Conflict("The updated blocked period overlaps with an existing blocked period.");

        entity.StartDateTime = newStart;
        entity.EndDateTime = newEnd;

        if (!string.IsNullOrWhiteSpace(dto.Description))
            entity.Description = dto.Description;

        await _dbContext.SaveChangesAsync();

        var BookingRuleBlockedPeriodDto = new BookingRuleBlockedPeriodDto
        {
            Id = entity.Id,
            StartDateTime = entity.StartDateTime,
            EndDateTime = entity.EndDateTime,
            Description = entity.Description
        };

        return ServiceResult<BookingRuleBlockedPeriodDto>.SuccessResult(BookingRuleBlockedPeriodDto);
    }

    public async Task<ServiceResult<bool>> DeleteBlockedPeriodAsync(Guid id)
    {
        var entity = await _dbContext.BookingRuleBlockedPeriod.FindAsync(id);
        if (entity == null)
            return ServiceResult<bool>.NotFound("Blocked period not found.");

        _dbContext.BookingRuleBlockedPeriod.Remove(entity);
        await _dbContext.SaveChangesAsync();

        return ServiceResult<bool>.NoContentResult("Blocked period deleted.");
    }
}
