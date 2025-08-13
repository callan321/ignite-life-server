using FluentAssertions;
using IgniteLifeApi.Application.Dtos.BookingRuleBlockedPeriod;
using IgniteLifeApi.Application.Validators.BookingRules;
using IgniteLifeApi.Domain.Constants;

namespace IgniteLifeApi.Tests.Tests.Validators.BookingRules
{
    public class UpdateBookingRuleBlockedPeriodDtoValidatorTests
    {
        private readonly UpdateBookingRuleBlockedPeriodDtoValidator _validator = new();

        [Fact]
        public void ShouldFail_WhenOnlyStartDateProvided()
        {
            var dto = new UpdateBookingRuleBlockedPeriodDto
            {
                StartDateTimeUtc = DateTime.UtcNow,
                EndDateTimeUtc = null
            };

            var result = _validator.Validate(dto);
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == string.Empty);
        }

        [Fact]
        public void ShouldFail_WhenOnlyEndDateProvided()
        {
            var dto = new UpdateBookingRuleBlockedPeriodDto
            {
                StartDateTimeUtc = null,
                EndDateTimeUtc = DateTime.UtcNow.AddDays(1)
            };

            var result = _validator.Validate(dto);
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == string.Empty);
        }

        [Fact]
        public void ShouldFail_WhenStartIsAfterOrEqualToEnd()
        {
            var start = DateTime.UtcNow.AddDays(2);
            var end = DateTime.UtcNow.AddDays(1);

            var dto = new UpdateBookingRuleBlockedPeriodDto
            {
                StartDateTimeUtc = start,
                EndDateTimeUtc = end
            };

            var result = _validator.Validate(dto);
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == string.Empty);
        }

        [Fact]
        public void ShouldPass_WhenBothDatesAreNull()
        {
            var dto = new UpdateBookingRuleBlockedPeriodDto
            {
                StartDateTimeUtc = null,
                EndDateTimeUtc = null
            };

            var result = _validator.Validate(dto);
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void ShouldPass_WhenStartIsBeforeEnd()
        {
            var dto = new UpdateBookingRuleBlockedPeriodDto
            {
                StartDateTimeUtc = DateTime.UtcNow,
                EndDateTimeUtc = DateTime.UtcNow.AddDays(1)
            };

            var result = _validator.Validate(dto);
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void ShouldFail_WhenDescriptionExceedsMaxLength()
        {
            var dto = new UpdateBookingRuleBlockedPeriodDto
            {
                Description = new string('x', FieldLengths.Description + 1)
            };

            var result = _validator.Validate(dto);
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(dto.Description));
        }

        [Fact]
        public void ShouldPass_WhenDescriptionIsAtMaxLength()
        {
            var dto = new UpdateBookingRuleBlockedPeriodDto
            {
                Description = new string('x', FieldLengths.Description)
            };

            var result = _validator.Validate(dto);
            result.IsValid.Should().BeTrue();
        }
    }
}