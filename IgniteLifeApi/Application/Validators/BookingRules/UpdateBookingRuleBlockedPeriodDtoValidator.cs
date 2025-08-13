using FluentValidation;
using IgniteLifeApi.Application.Dtos.BookingRuleBlockedPeriod;
using IgniteLifeApi.Domain.Constants;

namespace IgniteLifeApi.Application.Validators.BookingRules
{
    public class UpdateBookingRuleBlockedPeriodDtoValidator
        : AbstractValidator<UpdateBookingRuleBlockedPeriodDto>
    {
        public UpdateBookingRuleBlockedPeriodDtoValidator()
        {
            RuleFor(x => x.Description)
                .MaximumLength(FieldLengths.ShortText)
                .WithMessage("Description is too long.");

            RuleFor(x => x)
                .Must(x =>
                {
                    var hasStart = x.StartDateTimeUtc.HasValue;
                    var hasEnd = x.EndDateTimeUtc.HasValue;

                    // Both must be present or neither
                    if (hasStart != hasEnd)
                        return false;

                    // If both present, Start must be before End
                    if (hasStart && hasEnd)
                        return x.StartDateTimeUtc < x.EndDateTimeUtc;

                    // Neither is present, that's valid
                    return true;
                })
                .WithMessage("If providing date fields, both Start and End must be provided and Start must be before End.");
        }
    }
}
