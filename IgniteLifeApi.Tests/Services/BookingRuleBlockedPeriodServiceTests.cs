using FluentAssertions;
using IgniteLifeApi.Dtos.BookingRuleBlockedPeriod;
using IgniteLifeApi.Services.Common;
using IgniteLifeApi.Tests.TestInfrastructure;
using Xunit;
using Xunit.Abstractions;

namespace IgniteLifeApi.Tests.Services
{
    public class BookingRuleBlockedPeriodServiceTests() : UnitTestBase()
    {
        [Fact(DisplayName = "Should fail when StartDateTime is after EndDateTime")]
        public async Task Should_Fail_When_StartDate_After_EndDate()
        {
            // Arrange
            var dto = new CreateBookingRuleBlockedPeriodDto
            {
                StartDateTime = DateTime.UtcNow.AddHours(2),
                EndDateTime = DateTime.UtcNow.AddHours(1)
            };

            // Act
            var result = await BlockedPeriodService.CreateBlockedPeriodAsync(dto);

            // Assert
            result.Status.Should().Be(ServiceResultStatus.BadRequest);
            result.Message.Should().Contain("StartTime must be before EndTime");
        }
    }
}
