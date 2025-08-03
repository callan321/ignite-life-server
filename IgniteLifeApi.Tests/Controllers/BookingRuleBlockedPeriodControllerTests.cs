using FluentAssertions;
using IgniteLifeApi.Controllers;
using IgniteLifeApi.Dtos.BookingRuleBlockedPeriod;
using IgniteLifeApi.Tests.TestInfrastructure;
using IgniteLifeApi.Tests.Utilities;
using System.Net;
using System.Net.Http.Json;

namespace IgniteLifeApi.Tests.Controllers
{
    [Collection("IntegrationTests")]
    public class BookingRuleBlockedPeriodControllerTests(ApiPostgresTestApplicationFactory factory)
    {
        private readonly HttpClient _client = factory.CreateClient();
        private readonly string _baseUrl = ApiRoutes.ForController<BookingRuleBlockedPeriodController>();

        [Fact]
        public async Task CreateBlockedPeriod_ShouldReturnCreated()
        {
            // Arrange
            var dto = new CreateBookingRuleBlockedPeriodDto
            {
                StartDateTime = DateTime.UtcNow.AddDays(1),
                EndDateTime = DateTime.UtcNow.AddDays(2),
                Description = "Test Block"
            };

            // Act
            var response = await _client.PostAsJsonAsync(_baseUrl, dto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var created = await response.Content.ReadFromJsonAsync<BookingRuleBlockedPeriodDto>();
            created.Should().NotBeNull();
        }

        [Fact]
        public void TrueShouldBeTrue()
        {
            true.Should().BeTrue();
        }
    }
}
