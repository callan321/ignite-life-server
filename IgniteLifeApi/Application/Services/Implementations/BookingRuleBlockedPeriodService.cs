using IgniteLifeApi.Application.Dtos.BookingRuleBlockedPeriod;
using IgniteLifeApi.Application.Services.Common;
using IgniteLifeApi.Domain.Entities;
using IgniteLifeApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IgniteLifeApi.Application.Services.Implementations
{
    public class BookingRuleBlockedPeriodService(ApplicationDbContext dbContext)
    {
        private readonly ApplicationDbContext _dbContext = dbContext;

        public async Task<ServiceResult<List<BookingRuleBlockedPeriodDto>>> GetAllBookingRuleBlockedPeriodsAsync()
        {
            var blockedPeriods = await _dbContext.BookingRuleBlockedPeriod
                .OrderBy(b => b.StartDateTime)
                .ToListAsync();

            var dtos = blockedPeriods.Select(MapToDto).ToList();
            return ServiceResult<List<BookingRuleBlockedPeriodDto>>.SuccessResult(dtos);
        }

        public async Task<ServiceResult<BookingRuleBlockedPeriodDto>> CreateBlockedPeriodAsync(CreateBookingRuleBlockedPeriodDto dto)
        {
            if (await IsOverlappingWithExistingAsync(dto.StartDateTime!.Value, dto.EndDateTime!.Value))
                return ServiceResult<BookingRuleBlockedPeriodDto>.Conflict("The blocked period overlaps with an existing blocked period.");

            var entity = new BookingRuleBlockedPeriod
            {
                Id = Guid.NewGuid(),
                StartDateTime = dto.StartDateTime.Value,
                EndDateTime = dto.EndDateTime.Value,
                Description = NormalizeDescription(dto.Description)
            };

            _dbContext.BookingRuleBlockedPeriod.Add(entity);
            await _dbContext.SaveChangesAsync();

            return ServiceResult<BookingRuleBlockedPeriodDto>.CreatedResult(MapToDto(entity), "Blocked period created.");
        }

        public async Task<ServiceResult<BookingRuleBlockedPeriodDto>> UpdateBlockedPeriodAsync(Guid id, UpdateBookingRuleBlockedPeriodDto dto)
        {
            var entity = await _dbContext.BookingRuleBlockedPeriod.FindAsync(id);
            if (entity == null)
                return ServiceResult<BookingRuleBlockedPeriodDto>.NotFound("Blocked period not found.");

            if (await IsOverlappingWithExistingAsync(dto.StartDateTime ?? entity.StartDateTime,
                                                     dto.EndDateTime ?? entity.EndDateTime, id))
                return ServiceResult<BookingRuleBlockedPeriodDto>.Conflict("The updated blocked period overlaps with an existing blocked period.");

            entity.StartDateTime = dto.StartDateTime ?? entity.StartDateTime;
            entity.EndDateTime = dto.EndDateTime ?? entity.EndDateTime;
            entity.Description = NormalizeDescription(dto.Description) ?? entity.Description;

            await _dbContext.SaveChangesAsync();

            return ServiceResult<BookingRuleBlockedPeriodDto>.SuccessResult(MapToDto(entity));
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

        // ---------- Private Helpers ----------

        private static BookingRuleBlockedPeriodDto MapToDto(BookingRuleBlockedPeriod entity) => new()
        {
            Id = entity.Id,
            StartDateTime = entity.StartDateTime,
            EndDateTime = entity.EndDateTime,
            Description = entity.Description
        };

        private async Task<bool> IsOverlappingWithExistingAsync(DateTime start, DateTime end, Guid? excludeId = null)
        {
            return await _dbContext.BookingRuleBlockedPeriod.AnyAsync(b =>
                (excludeId == null || b.Id != excludeId) && b.StartDateTime < end && start < b.EndDateTime);
        }

        private static string? NormalizeDescription(string? description) =>
            string.IsNullOrWhiteSpace(description) ? null : description.Trim();
    }
}