using IgniteLifeApi.Application.Dtos.BookingRuleBlockedPeriod;
using IgniteLifeApi.Application.Dtos.BookingRules;
using IgniteLifeApi.Application.Services.Common;
using IgniteLifeApi.Domain.Entities;
using IgniteLifeApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IgniteLifeApi.Application.Services.Implementations
{
    public class BookingRuleService
    {
        private readonly ApplicationDbContext _dbContext;

        public BookingRuleService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // ---------- Public Methods ----------
        public async Task<ServiceResult<BookingRulesDto>> GetBookingRulesAsync(CancellationToken cancellationToken = default)
        {
            var rules = await GetSingletonRulesAsync(cancellationToken);
            if (rules == null)
                return ServiceResult<BookingRulesDto>.NotFound("No booking rules found.");

            return ServiceResult<BookingRulesDto>.SuccessResult(MapToDto(rules));
        }

        public async Task<ServiceResult<BookingRulesDto>> UpdateBookingRulesAsync(Guid id, UpdateBookingRulesDto dto, CancellationToken cancellationToken = default)
        {
            var rules = await _dbContext.BookingRules
                .Include(r => r.OpeningHours)
                .SingleOrDefaultAsync(r => r.Id == id, cancellationToken);

            if (rules == null)
                return ServiceResult<BookingRulesDto>.NotFound("Booking rules not found.");

            if (dto.SlotDurationMinutes.HasValue)
                rules.SlotDurationMinutes = dto.SlotDurationMinutes.Value;

            if (dto.BufferBetweenBookingsMinutes.HasValue)
                rules.BufferBetweenBookingsMinutes = dto.BufferBetweenBookingsMinutes.Value;

            if (dto.MaxAdvanceBookingDays.HasValue)
                rules.MaxAdvanceBookingDays = dto.MaxAdvanceBookingDays.Value;

            if (dto.MinAdvanceBookingHours.HasValue)
                rules.MinAdvanceBookingHours = dto.MinAdvanceBookingHours.Value;

            if (dto.OpeningHours != null && dto.OpeningHours.Count > 0)
            {
                var byId = rules.OpeningHours.ToDictionary(h => h.Id);

                foreach (var ohDto in dto.OpeningHours)
                {
                    if (!byId.TryGetValue(ohDto.Id, out var oh) || oh.BookingRulesId != id)
                        return ServiceResult<BookingRulesDto>.NotFound($"Opening hour '{ohDto.Id}' was not found.");

                    if (ohDto.OpenTimeUtc.HasValue) oh.OpenTimeUtc = ohDto.OpenTimeUtc.Value;
                    if (ohDto.CloseTimeUtc.HasValue) oh.CloseTimeUtc = ohDto.CloseTimeUtc.Value;
                    if (ohDto.IsClosed.HasValue) oh.IsClosed = ohDto.IsClosed.Value;
                }
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            return await ReturnUpdatedRulesAsync(id, "Booking Rules updated.", cancellationToken);
        }

        public async Task<ServiceResult<BookingRulesDto>> CreateBlockedPeriodAsync(CreateBookingRuleBlockedPeriodDto dto, CancellationToken cancellationToken = default)
        {
            var rules = await GetSingletonRulesAsync(cancellationToken);
            if (rules == null)
                return ServiceResult<BookingRulesDto>.NotFound("Booking ruleset not found.");

            if (await IsOverlappingWithExistingAsync(dto.StartDateTimeUtc!.Value, dto.EndDateTimeUtc!.Value, rules.Id, null, cancellationToken))
                return ServiceResult<BookingRulesDto>.Conflict("The blocked period overlaps with an existing blocked period.");

            var entity = new BookingRuleBlockedPeriod
            {
                BookingRulesId = rules.Id,
                StartDateTimeUtc = dto.StartDateTimeUtc.Value,
                EndDateTimeUtc = dto.EndDateTimeUtc.Value,
                Description = NormalizeDescription(dto.Description)
            };

            _dbContext.BookingRuleBlockedPeriods.Add(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return await ReturnUpdatedRulesAsync(entity.BookingRulesId, "Blocked period created.", cancellationToken, true);
        }

        public async Task<ServiceResult<BookingRulesDto>> UpdateBlockedPeriodAsync(Guid id, UpdateBookingRuleBlockedPeriodDto dto, CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.BookingRuleBlockedPeriods.FindAsync(new object[] { id }, cancellationToken);
            if (entity == null)
                return ServiceResult<BookingRulesDto>.NotFound("Blocked period not found.");

            if (await IsOverlappingWithExistingAsync(dto.StartDateTimeUtc ?? entity.StartDateTimeUtc,
                                                     dto.EndDateTimeUtc ?? entity.EndDateTimeUtc,
                                                     entity.BookingRulesId,
                                                     id,
                                                     cancellationToken))
                return ServiceResult<BookingRulesDto>.Conflict("The updated blocked period overlaps with an existing blocked period.");

            entity.StartDateTimeUtc = dto.StartDateTimeUtc ?? entity.StartDateTimeUtc;
            entity.EndDateTimeUtc = dto.EndDateTimeUtc ?? entity.EndDateTimeUtc;
            entity.Description = NormalizeDescription(dto.Description) ?? entity.Description;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return await ReturnUpdatedRulesAsync(entity.BookingRulesId, "Blocked period updated.", cancellationToken);
        }

        public async Task<ServiceResult<BookingRulesDto>> DeleteBlockedPeriodAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.BookingRuleBlockedPeriods.FindAsync(new object[] { id }, cancellationToken);
            if (entity == null)
                return ServiceResult<BookingRulesDto>.NotFound("Blocked period not found.");

            _dbContext.BookingRuleBlockedPeriods.Remove(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return await ReturnUpdatedRulesAsync(entity.BookingRulesId, "Blocked period deleted.", cancellationToken);
        }

        // ---------- Private Helpers ----------

        private async Task<BookingRules?> GetSingletonRulesAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.BookingRules
                .Include(r => r.OpeningHours)
                .Include(r => r.BlockedPeriods)
                .SingleOrDefaultAsync(cancellationToken);
        }

        private async Task<ServiceResult<BookingRulesDto>> ReturnUpdatedRulesAsync(Guid id, string message, CancellationToken cancellationToken = default, bool created = false)
        {
            var updated = await _dbContext.BookingRules
                .Include(r => r.OpeningHours)
                .Include(r => r.BlockedPeriods)
                .SingleOrDefaultAsync(r => r.Id == id, cancellationToken);

            if (updated == null)
                return ServiceResult<BookingRulesDto>.NotFound("Booking rules not found after update.");

            if (created)
                return ServiceResult<BookingRulesDto>.CreatedResult(MapToDto(updated), message);
            return ServiceResult<BookingRulesDto>.SuccessResult(MapToDto(updated), message);
        }

        private static BookingRulesDto MapToDto(BookingRules entity) => new()
        {
            Id = entity.Id,
            MaxAdvanceBookingDays = entity.MaxAdvanceBookingDays,
            BufferBetweenBookingsMinutes = entity.BufferBetweenBookingsMinutes,
            SlotDurationMinutes = entity.SlotDurationMinutes,
            MinAdvanceBookingHours = entity.MinAdvanceBookingHours,
            OpeningHours = entity.OpeningHours.Select(h => new BookingRulesOpeningHourDto
            {
                Id = h.Id,
                DayOfWeek = h.DayOfWeek,
                OpenTimeUtc = h.OpenTimeUtc,
                CloseTimeUtc = h.CloseTimeUtc,
                IsClosed = h.IsClosed
            }).ToList(),
            BlockedPeriods = entity.BlockedPeriods.Select(b => new BookingRulesBlockedPeriodDto
            {
                Id = b.Id,
                StartDateTimeUtc = b.StartDateTimeUtc,
                EndDateTimeUtc = b.EndDateTimeUtc,
                Description = b.Description
            }).ToList()
        };

        private async Task<bool> IsOverlappingWithExistingAsync(DateTime start, DateTime end, Guid bookingRulesId, Guid? excludeId = null, CancellationToken cancellationToken = default)
        {
            return await _dbContext.BookingRuleBlockedPeriods.AnyAsync(b =>
                b.BookingRulesId == bookingRulesId &&
                (excludeId == null || b.Id != excludeId) &&
                b.StartDateTimeUtc < end &&
                start < b.EndDateTimeUtc,
                cancellationToken);
        }

        private static string? NormalizeDescription(string? description) =>
            string.IsNullOrWhiteSpace(description) ? null : description.Trim();
    }
}
