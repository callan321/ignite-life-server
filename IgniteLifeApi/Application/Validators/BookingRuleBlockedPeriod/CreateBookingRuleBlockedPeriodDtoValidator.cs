using FluentValidation;
using IgniteLifeApi.Application.Dtos.BookingRuleBlockedPeriod;

namespace IgniteLifeApi.Application.Validators.BookingRuleBlockedPeriod
{
    public class CreateBookingRuleBlockedPeriodDtoValidator
        : AbstractValidator<CreateBookingRuleBlockedPeriodDto>
    {
        public CreateBookingRuleBlockedPeriodDtoValidator()
        {
            RuleFor(x => x.StartDateTime)
                .NotNull().WithMessage("Start time is required.");

            RuleFor(x => x.EndDateTime)
                .NotNull().WithMessage("End time is required.");

            RuleFor(x => x)
                .Must(x => x.StartDateTime < x.EndDateTime)
                .WithMessage("Start time must be before end time.");

            RuleFor(x => x.Description)
                .MaximumLength(1000)
                .WithMessage("Description cannot exceed 1000 characters.");
        }
    }
}
