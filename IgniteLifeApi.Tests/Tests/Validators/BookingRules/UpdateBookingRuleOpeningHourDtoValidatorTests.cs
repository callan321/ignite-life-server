using FluentAssertions;
using IgniteLifeApi.Application.Dtos.BookingRules;
using IgniteLifeApi.Application.Validators.BookingRules;

namespace IgniteLifeApi.Tests.Tests.Validators.BookingRules
{
    public class UpdateBookingRuleOpeningHourDtoValidatorTests
    {
        private readonly UpdateBookingRuleOpeningHourDtoValidator _validator = new();

        [Fact]
        public void Id_ShouldFail_WhenEmptyGuid()
        {
            var dto = new UpdateBookingRuleOpeningHourDto { Id = Guid.Empty };
            var result = _validator.Validate(dto);
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(dto.Id));
        }

        [Fact]
        public void Id_ShouldPass_WhenValidGuid()
        {
            var dto = new UpdateBookingRuleOpeningHourDto { Id = Guid.NewGuid() };
            var result = _validator.Validate(dto);
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void ShouldFail_WhenOnlyOpenTimeProvided()
        {
            var dto = new UpdateBookingRuleOpeningHourDto
            {
                Id = Guid.NewGuid(),
                OpenTimeUtc = new TimeOnly(9, 0)
            };
            var result = _validator.Validate(dto);
            result.IsValid.Should().BeFalse();
        }

        [Fact]
        public void ShouldFail_WhenOnlyCloseTimeProvided()
        {
            var dto = new UpdateBookingRuleOpeningHourDto
            {
                Id = Guid.NewGuid(),
                CloseTimeUtc = new TimeOnly(17, 0)
            };
            var result = _validator.Validate(dto);
            result.IsValid.Should().BeFalse();
        }

        [Fact]
        public void ShouldFail_WhenOpenTimeEqualsCloseTime()
        {
            var dto = new UpdateBookingRuleOpeningHourDto
            {
                Id = Guid.NewGuid(),
                OpenTimeUtc = new TimeOnly(9, 0),
                CloseTimeUtc = new TimeOnly(9, 0)
            };
            var result = _validator.Validate(dto);
            result.IsValid.Should().BeFalse();
        }

        [Fact]
        public void ShouldFail_WhenOpenTimeAfterCloseTime()
        {
            var dto = new UpdateBookingRuleOpeningHourDto
            {
                Id = Guid.NewGuid(),
                OpenTimeUtc = new TimeOnly(17, 0),
                CloseTimeUtc = new TimeOnly(9, 0)
            };
            var result = _validator.Validate(dto);
            result.IsValid.Should().BeFalse();
        }

        [Fact]
        public void ShouldPass_WhenBothTimesNull()
        {
            var dto = new UpdateBookingRuleOpeningHourDto
            {
                Id = Guid.NewGuid(),
                OpenTimeUtc = null,
                CloseTimeUtc = null
            };
            var result = _validator.Validate(dto);
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void ShouldPass_WhenOpenTimeBeforeCloseTime()
        {
            var dto = new UpdateBookingRuleOpeningHourDto
            {
                Id = Guid.NewGuid(),
                OpenTimeUtc = new TimeOnly(9, 0),
                CloseTimeUtc = new TimeOnly(17, 0)
            };
            var result = _validator.Validate(dto);
            result.IsValid.Should().BeTrue();
        }
    }
}