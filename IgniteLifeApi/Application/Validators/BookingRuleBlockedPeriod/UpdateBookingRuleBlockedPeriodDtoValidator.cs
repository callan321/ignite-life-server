using FluentValidation;
using IgniteLifeApi.Application.Dtos.BookingRuleBlockedPeriod;

namespace IgniteLifeApi.Application.Validators.BookingRuleBlockedPeriod
{
    public class UpdateBookingRuleBlockedPeriodDtoValidator
        : AbstractValidator<UpdateBookingRuleBlockedPeriodDto>
    {
        public UpdateBookingRuleBlockedPeriodDtoValidator()
        {
            RuleFor(x => x.Description)
                .MaximumLength(1000)
                .WithMessage("Description cannot exceed 1000 characters.");

            RuleFor(x => x)
                .Must(x =>
                {
                    var hasStart = x.StartDateTime.HasValue;
                    var hasEnd = x.EndDateTime.HasValue;

                    // Both must be present or neither
                    if (hasStart != hasEnd)
                        return false;

                    // If both present, Start must be before End
                    if (hasStart && hasEnd)
                        return x.StartDateTime < x.EndDateTime;

                    // Neither is present, that's valid
                    return true;
                })
                .WithMessage("If providing date fields, both Start and End must be provided and Start must be before End.");
        }
    }
}
