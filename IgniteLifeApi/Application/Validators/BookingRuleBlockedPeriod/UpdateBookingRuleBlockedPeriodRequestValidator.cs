using FluentValidation;
using IgniteLifeApi.Application.Dtos.BookingRuleBlockedPeriod;

namespace IgniteLifeApi.Application.Validators.BookingRuleBlockedPeriod
{
    public class UpdateBookingRuleBlockedPeriodRequestValidator
        : AbstractValidator<UpdateBookingRuleBlockedPeriodRequest>
    {
        public UpdateBookingRuleBlockedPeriodRequestValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Id is required.");

            RuleFor(x => x)
                .Must(x =>
                    (x.StartDateTime.HasValue && x.EndDateTime.HasValue && x.StartDateTime < x.EndDateTime) ||
                    (!x.StartDateTime.HasValue && !x.EndDateTime.HasValue))
                .WithMessage("Start time must be before end time.");

            RuleFor(x => x.Description)
                .MaximumLength(1000)
                .When(x => !string.IsNullOrEmpty(x.Description))
                .WithMessage("Description cannot exceed 1000 characters.");
        }
    }
}