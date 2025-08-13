using FluentValidation;
using IgniteLifeApi.Application.Dtos.BookingRuleBlockedPeriod;
using IgniteLifeApi.Domain.Constants;

namespace IgniteLifeApi.Application.Validators.BookingRules
{
    public class CreateBookingRuleBlockedPeriodDtoValidator
        : AbstractValidator<CreateBookingRuleBlockedPeriodDto>
    {
        public CreateBookingRuleBlockedPeriodDtoValidator()
        {
            RuleFor(x => x)
                .Must(x => x.StartDateTimeUtc < x.EndDateTimeUtc)
                .WithMessage("Start time must be before end time.");

            RuleFor(x => x.Description)
                .MaximumLength(FieldLengths.Description)
                .WithMessage($"Description cannot exceed {FieldLengths.Description} characters.");
        }
    }
}
