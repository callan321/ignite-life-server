using FluentAssertions;
using IgniteLifeApi.Application.Dtos.BookingRuleBlockedPeriod;
using IgniteLifeApi.Application.Validators.BookingRules;
using IgniteLifeApi.Domain.Constants;

namespace IgniteLifeApi.Tests.Tests.Validators.BookingRules
{
    public class CreateBookingRuleBlockedPeriodDtoValidatorTests
    {
        private readonly CreateBookingRuleBlockedPeriodDtoValidator _validator = new();

        [Fact]
        public void ShouldFail_WhenStartDateTimeIsAfterEndDateTime()
        {
            var dto = new CreateBookingRuleBlockedPeriodDto
            {
                StartDateTimeUtc = DateTime.UtcNow.AddDays(2),
                EndDateTimeUtc = DateTime.UtcNow.AddDays(1),
                Description = "Invalid"
            };

            var result = _validator.Validate(dto);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == string.Empty);
        }

        [Fact]
        public void ShouldFail_WhenStartDateTimeEqualsEndDateTime()
        {
            var now = DateTime.UtcNow;
            var dto = new CreateBookingRuleBlockedPeriodDto
            {
                StartDateTimeUtc = now,
                EndDateTimeUtc = now,
                Description = "Same time"
            };

            var result = _validator.Validate(dto);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == string.Empty);
        }

        [Fact]
        public void ShouldPass_WhenStartIsBeforeEnd()
        {
            var dto = new CreateBookingRuleBlockedPeriodDto
            {
                StartDateTimeUtc = DateTime.UtcNow,
                EndDateTimeUtc = DateTime.UtcNow.AddDays(1),
                Description = "Valid"
            };

            var result = _validator.Validate(dto);

            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void ShouldFail_WhenDescriptionExceedsMaxLength()
        {
            var dto = new CreateBookingRuleBlockedPeriodDto
            {
                StartDateTimeUtc = DateTime.UtcNow,
                EndDateTimeUtc = DateTime.UtcNow.AddDays(1),
                Description = new string('a', FieldLengths.Description + 1)
            };

            var result = _validator.Validate(dto);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(dto.Description));
        }

        [Fact]
        public void ShouldPass_WhenDescriptionIsAtMaxLength()
        {
            var dto = new CreateBookingRuleBlockedPeriodDto
            {
                StartDateTimeUtc = DateTime.UtcNow,
                EndDateTimeUtc = DateTime.UtcNow.AddDays(1),
                Description = new string('a', FieldLengths.Description)
            };

            var result = _validator.Validate(dto);

            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void ShouldPass_WhenDescriptionIsNull()
        {
            var dto = new CreateBookingRuleBlockedPeriodDto
            {
                StartDateTimeUtc = DateTime.UtcNow,
                EndDateTimeUtc = DateTime.UtcNow.AddDays(1),
                Description = null
            };

            var result = _validator.Validate(dto);

            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void ShouldPass_WhenDescriptionIsEmpty()
        {
            var dto = new CreateBookingRuleBlockedPeriodDto
            {
                StartDateTimeUtc = DateTime.UtcNow,
                EndDateTimeUtc = DateTime.UtcNow.AddDays(1),
                Description = string.Empty
            };

            var result = _validator.Validate(dto);

            result.IsValid.Should().BeTrue();
        }
    }
}