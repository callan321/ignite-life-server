using FluentValidation;
using IgniteLifeApi.Application.Dtos.BowenServices;
using IgniteLifeApi.Domain.Constants;

public sealed class CreateBowenServiceRequestValidator : AbstractValidator<CreateBowenServiceDto>
{
    public CreateBowenServiceRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(FieldLengths.Title);

        RuleFor(x => x.Price)
            .GreaterThan(0m);

        RuleFor(x => x.DurationMinutes)
            .InclusiveBetween(0, 360);

        RuleFor(x => x.Description)
            .MaximumLength(FieldLengths.Description);

        RuleFor(x => x.SessionCount)
            .Null().When(x => !x.IsMultiSession)
            .NotNull().When(x => x.IsMultiSession)
            .GreaterThan(1).When(x => x.IsMultiSession);

        RuleFor(x => x.MaxGroupSize)
            .Null().When(x => !x.IsGroupSession)
            .NotNull().When(x => x.IsGroupSession)
            .GreaterThan(1).When(x => x.IsGroupSession);

        RuleFor(x => x.ImageAltText)
            .MaximumLength(FieldLengths.ImageAltText);
    }
}
