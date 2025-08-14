using System.Linq;
using FluentValidation;
using IgniteLifeApi.Application.Dtos.BowenServices;
using IgniteLifeApi.Domain.Constants;

public sealed class UpdateBowenServiceRequestValidator : AbstractValidator<UpdateBowenServiceDto>
{
    public UpdateBowenServiceRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(FieldLengths.Title);

        RuleFor(x => x.Price)
            .GreaterThan(0m);

        // Mirrors your create rule
        RuleFor(x => x.DurationMinutes)
            .InclusiveBetween(0, 360);

        RuleFor(x => x.Description)
            .MaximumLength(FieldLengths.Description);

        RuleFor(x => x.ImageAltText)
            .MaximumLength(FieldLengths.ImageAltText);

    }
}
