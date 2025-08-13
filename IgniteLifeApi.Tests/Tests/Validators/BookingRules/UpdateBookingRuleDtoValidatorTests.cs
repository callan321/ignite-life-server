using FluentAssertions;
using IgniteLifeApi.Application.Dtos.BookingRules;
using IgniteLifeApi.Application.Validators.BookingRules;
using IgniteLifeApi.Domain.Enums;

namespace IgniteLifeApi.Tests.Tests.Validators.BookingRules
{
    public class UpdateBookingRuleDtoValidatorTests
    {
        private readonly UpdateBookingRuleDtoValidator _validator = new();

        [Fact]
        public void MaxAdvanceBookingDays_ShouldFail_WhenZero()
        {
            var dto = new UpdateBookingRulesDto { MaxAdvanceBookingDays = 0 };
            var result = _validator.Validate(dto);
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(dto.MaxAdvanceBookingDays));
        }

        [Fact]
        public void MaxAdvanceBookingDays_ShouldPass_WhenGreaterThanZero()
        {
            var dto = new UpdateBookingRulesDto { MaxAdvanceBookingDays = 5 };
            var result = _validator.Validate(dto);
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void BufferBetweenBookingsMinutes_ShouldFail_WhenInvalidEnum()
        {
            var dto = new UpdateBookingRulesDto { BufferBetweenBookingsMinutes = 999 };
            var result = _validator.Validate(dto);
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(dto.BufferBetweenBookingsMinutes));
        }

        [Fact]
        public void BufferBetweenBookingsMinutes_ShouldPass_WhenValidEnum()
        {
            var validValue = (int)BookingBufferMinutes.Minutes30;
            var dto = new UpdateBookingRulesDto { BufferBetweenBookingsMinutes = validValue };
            var result = _validator.Validate(dto);
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void BufferBetweenBookingsMinutes_ShouldPass_WhenNull()
        {
            var dto = new UpdateBookingRulesDto { BufferBetweenBookingsMinutes = null };
            var result = _validator.Validate(dto);
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void SlotDurationMinutes_ShouldFail_WhenInvalidEnum()
        {
            var dto = new UpdateBookingRulesDto { SlotDurationMinutes = 1234 };
            var result = _validator.Validate(dto);
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(dto.SlotDurationMinutes));
        }

        [Fact]
        public void SlotDurationMinutes_ShouldPass_WhenValidEnum()
        {
            var validValue = (int)BookingSlotDuration.Minutes60;
            var dto = new UpdateBookingRulesDto { SlotDurationMinutes = validValue };
            var result = _validator.Validate(dto);
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void SlotDurationMinutes_ShouldPass_WhenNull()
        {
            var dto = new UpdateBookingRulesDto { SlotDurationMinutes = null };
            var result = _validator.Validate(dto);
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void MinAdvanceBookingHours_ShouldFail_WhenNegative()
        {
            var dto = new UpdateBookingRulesDto { MinAdvanceBookingHours = -1 };
            var result = _validator.Validate(dto);
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(dto.MinAdvanceBookingHours));
        }

        [Fact]
        public void MinAdvanceBookingHours_ShouldPass_WhenZero()
        {
            var dto = new UpdateBookingRulesDto { MinAdvanceBookingHours = 0 };
            var result = _validator.Validate(dto);
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void MinAdvanceBookingHours_ShouldPass_WhenPositive()
        {
            var dto = new UpdateBookingRulesDto { MinAdvanceBookingHours = 5 };
            var result = _validator.Validate(dto);
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Should_Call_NestedValidator_ForOpeningHours()
        {
            var dto = new UpdateBookingRulesDto
            {
                OpeningHours = [new UpdateBookingRuleOpeningHourDto { Id = Guid.Empty }]
            };

            var result = _validator.Validate(dto);
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName.Contains("OpeningHours"));
        }
    }
}