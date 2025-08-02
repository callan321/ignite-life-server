using IgniteLifeApi.Data;
using IgniteLifeApi.Dtos.BookingRuleBlockedPeriod;
using IgniteLifeApi.Models;
using IgniteLifeApi.Services.Common;
using Microsoft.EntityFrameworkCore;

namespace IgniteLifeApi.Services;

public class BookingRuleBlockedPeriodService(ApplicationDbContext dbContext, ILogger<BookingRuleBlockedPeriodService> logger)
{
    private readonly ApplicationDbContext _dbContext = dbContext;
    private readonly ILogger<BookingRuleBlockedPeriodService> _logger = logger;

    public Task<ServiceResult<List<BookingRuleBlockedPeriodDto>>> GetAllBookingRuleBlockedPeriodsAsync() =>
        ErrorHandler.ExecuteWithErrorHandlingAsync(async () =>
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
        }, _logger, "An unexpected error occurred while retrieving blocked periods.");

    public Task<ServiceResult<BookingRuleBlockedPeriodDto>> CreateBlockedPeriodAsync(CreateBookingRuleBlockedPeriodDto dto) =>
        ErrorHandler.ExecuteWithErrorHandlingAsync(async () =>
        {
            if (dto == null)
                return ServiceResult<BookingRuleBlockedPeriodDto>.BadRequest("Request body cannot be null.");

            if (dto.StartDateTime >= dto.EndDateTime)
                return ServiceResult<BookingRuleBlockedPeriodDto>.BadRequest("StartTime must be before EndTime.");

            var isOverlap = await _dbContext.BookingRuleBlockedPeriod.AnyAsync(b =>
                b.StartDateTime < dto.EndDateTime && dto.StartDateTime < b.EndDateTime);

            if (isOverlap)
                return ServiceResult<BookingRuleBlockedPeriodDto>.Conflict("The blocked period overlaps with an existing blocked period.");

            var entity = new BookingRuleBlockedPeriod
            {
                Id = Guid.NewGuid(),
                StartDateTime = dto.StartDateTime,
                EndDateTime = dto.EndDateTime,
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
        }, _logger, "An unexpected error occurred while creating the blocked period.");

    public Task<ServiceResult<BookingRuleBlockedPeriodDto>> UpdateBlockedPeriodAsync(UpdateBookingRuleBlockedPeriodDto dto) =>
        ErrorHandler.ExecuteWithErrorHandlingAsync(async () =>
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

            var resultDto = new BookingRuleBlockedPeriodDto
            {
                Id = entity.Id,
                StartDateTime = entity.StartDateTime,
                EndDateTime = entity.EndDateTime,
                Description = entity.Description
            };

            return ServiceResult<BookingRuleBlockedPeriodDto>.NoContentResult("Blocked period updated.");
        }, _logger, "An unexpected error occurred while updating the blocked period.");

    public Task<ServiceResult<bool>> DeleteBlockedPeriodAsync(Guid id) =>
        ErrorHandler.ExecuteWithErrorHandlingAsync(async () =>
        {
            var entity = await _dbContext.BookingRuleBlockedPeriod.FindAsync(id);
            if (entity == null)
                return ServiceResult<bool>.NotFound("Blocked period not found.");

            _dbContext.BookingRuleBlockedPeriod.Remove(entity);
            await _dbContext.SaveChangesAsync();

            return ServiceResult<bool>.NoContentResult("Blocked period deleted.");
        }, _logger, "An unexpected error occurred while deleting the blocked period.");
}
