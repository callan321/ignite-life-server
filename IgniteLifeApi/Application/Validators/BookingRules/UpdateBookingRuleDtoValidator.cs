using FluentValidation;
using IgniteLifeApi.Application.Dtos.BookingRules;
using IgniteLifeApi.Domain.Enums;

namespace IgniteLifeApi.Application.Validators.BookingRules
{
    public class UpdateBookingRuleDtoValidator : AbstractValidator<UpdateBookingRulesDto>
    {
        // Precompute the display strings once (cheap + keeps rules tidy).
        private static readonly string AllowedBufferMinutesText = BuildAllowedText<BookingBufferMinutes>();
        private static readonly string AllowedSlotDurationsText = BuildAllowedText<BookingSlotDuration>();

        public UpdateBookingRuleDtoValidator()
        {
            // Optional value; only validate when provided (> 0)
            RuleFor(x => x.MaxAdvanceBookingDays)
                .GreaterThan(0).WithMessage("Max advance booking days must be greater than zero.")
                .When(x => x.MaxAdvanceBookingDays != default);

            // Optional; if provided, must match an enum value
            RuleFor(x => x.BufferBetweenBookingsMinutes)
                .Must(v => v == null || Enum.IsDefined(typeof(BookingBufferMinutes), v))
                .WithMessage($"Buffer between bookings must be one of: {AllowedBufferMinutesText}.");

            // Optional; if provided, must match an enum value
            RuleFor(x => x.SlotDurationMinutes)
                .Must(v => v == null || Enum.IsDefined(typeof(BookingSlotDuration), v))
                .WithMessage($"Slot duration must be one of: {AllowedSlotDurationsText}.");

            // Optional value; only validate when provided (>= 0)
            RuleFor(x => x.MinAdvanceBookingHours)
                .GreaterThanOrEqualTo(0).WithMessage("Minimum advance booking hours must be zero or more.")
                .When(x => x.MinAdvanceBookingHours != default);

            // Validate nested OpeningHours collection if provided
            RuleForEach(x => x.OpeningHours)
                .SetValidator(new UpdateBookingRuleOpeningHourDtoValidator());
        }

        private static string BuildAllowedText<TEnum>() where TEnum : struct, Enum =>
            string.Join(", ",
                Enum.GetValues(typeof(TEnum))
                    .Cast<int>()
                    .Distinct()
                    .OrderBy(v => v));
    }
}
