using Server.Data;
using Server.Dtos;
using Server.Utils;

namespace Server.Services;

public class BookingRulesService(ApplicationDbContext context)
{
    private readonly ApplicationDbContext _dbContext = context;

    public async Task<ApiResult<ReadBookingRulesResponse>> GetDefaultBookingRulesAsync()
    {
        var defaultBookingRules = await BookingRulesUtils.GetDefaultBookingRules(_dbContext);
        return defaultBookingRules is null
            ? ApiResult<ReadBookingRulesResponse>.Fail("Default booking rules not found.")
            : ApiResult<ReadBookingRulesResponse>.Success(
                BookingRulesUtils.CreateReadBookingRulesResponse(defaultBookingRules)
            );
    }

    public async Task<ApiResult<ReadBookingRulesResponse>> UpdateDefaultBookingRulesAsync(UpdateBookingRulesRequest updateRequest)
    {
        var defaultBookingRules = await BookingRulesUtils.GetDefaultBookingRules(_dbContext);
        if (defaultBookingRules == null)
            return ApiResult<ReadBookingRulesResponse>.Fail("Default booking rules not found.");

        if (updateRequest.MaxAdvanceBookingDays.HasValue)
            defaultBookingRules.MaxAdvanceBookingDays = updateRequest.MaxAdvanceBookingDays.Value;

        if (updateRequest.BufferBetweenBookingsMinutes.HasValue)
            defaultBookingRules.BufferBetweenBookingsMinutes = updateRequest.BufferBetweenBookingsMinutes.Value;

        if (updateRequest.SlotDurationMinutes.HasValue)
            defaultBookingRules.SlotDurationMinutes = updateRequest.SlotDurationMinutes.Value;

        if (updateRequest.MinAdvanceBookingHours.HasValue)
            defaultBookingRules.MinAdvanceBookingHours = updateRequest.MinAdvanceBookingHours.Value;

        if (updateRequest.OpeningHours != null)
        {
            foreach (var openingHourUpdate in updateRequest.OpeningHours)
            {
                if (!DateTimeUtils.IsValidRange(openingHourUpdate.OpenTime, openingHourUpdate.CloseTime))
                {
                    return ApiResult<ReadBookingRulesResponse>.Fail(
                        $"Invalid time range for {openingHourUpdate.DayOfWeek}: open must be before close."
                    );
                }

                var existing = defaultBookingRules.OpeningHours
                    .FirstOrDefault(h => h.DayOfWeek == openingHourUpdate.DayOfWeek);

                if (existing != null)
                {
                    if (openingHourUpdate.OpenTime.HasValue)
                        existing.OpenTime = openingHourUpdate.OpenTime.Value;
                    if (openingHourUpdate.CloseTime.HasValue)
                        existing.CloseTime = openingHourUpdate.CloseTime.Value;
                }
            }
        }

        if (updateRequest.OpeningExceptions != null)
        {
            // TODO Handle updating opening exceptions here if needed
            // You can match by Id or recreate the list.
        }

        await _dbContext.SaveChangesAsync();

        return ApiResult<ReadBookingRulesResponse>.Success(
            BookingRulesUtils.CreateReadBookingRulesResponse(defaultBookingRules)
        );
    }
}
