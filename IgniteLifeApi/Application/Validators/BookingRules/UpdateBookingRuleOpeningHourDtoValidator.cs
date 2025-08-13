using FluentValidation;
using IgniteLifeApi.Application.Dtos.BookingRules;

namespace IgniteLifeApi.Application.Validators.BookingRules
{
    public class UpdateBookingRuleOpeningHourDtoValidator
        : AbstractValidator<UpdateBookingRuleOpeningHourDto>
    {
        public UpdateBookingRuleOpeningHourDtoValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Opening hour ID is required.");

            // Open/Close time consistency & ordering
            RuleFor(x => x)
                .Must(x =>
                {
                    var hasOpen = x.OpenTimeUtc.HasValue;
                    var hasClose = x.CloseTimeUtc.HasValue;

                    if (hasOpen != hasClose)
                        return false;

                    if (hasOpen && hasClose)
                        return x.OpenTimeUtc < x.CloseTimeUtc;

                    return true;
                })
                .WithMessage("If providing times, both open and close times must be provided and open must be before close.");
        }
    }
}
