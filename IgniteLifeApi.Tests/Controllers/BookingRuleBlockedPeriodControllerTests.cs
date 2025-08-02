using FluentAssertions;
using IgniteLifeApi.Controllers;
using IgniteLifeApi.Dtos.BookingRuleBlockedPeriod;
using IgniteLifeApi.Tests.TestInfrastructure;
using IgniteLifeApi.Tests.Utilities;
using System.Net;
using System.Net.Http.Json;
using Xunit.Abstractions;

namespace IgniteLifeApi.Tests.Controllers
{
    public class BookingRuleBlockedPeriodControllerTests(
        PostgresContainerFixture fixture) : IntegrationTestBase(fixture)
    {
        private readonly string _baseUrl = ApiRoutes.ForController<BookingRuleBlockedPeriodController>();

        [Fact(DisplayName = "Blocked Periods basic CRUD should work successfully")]
        public async Task BlockedPeriods_CRUD_Should_Work()
        {
            // CREATE
            var createDto = new CreateBookingRuleBlockedPeriodDto
            {
                StartDateTime = DateTime.UtcNow.AddDays(1),
                EndDateTime = DateTime.UtcNow.AddDays(2),
                Description = "Initial block"
            };

            var createResponse = await Client.PostAsJsonAsync(_baseUrl, createDto);
            createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

            var created = await createResponse.Content.ReadFromJsonAsync<BookingRuleBlockedPeriodDto>();
            created.Should().NotBeNull();
            created!.Id.Should().NotBeEmpty();
            created.Description.Should().Be("Initial block");

            // READ
            var getAllResponse = await Client.GetAsync(_baseUrl);
            getAllResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var allBlockedPeriods = await getAllResponse.Content.ReadFromJsonAsync<List<BookingRuleBlockedPeriodDto>>();
            allBlockedPeriods.Should().Contain(bp => bp.Id == created.Id);

            // UPDATE
            var updateDto = new UpdateBookingRuleBlockedPeriodDto
            {
                Id = created.Id,
                StartDateTime = DateTime.UtcNow.AddDays(3),
                EndDateTime = DateTime.UtcNow.AddDays(4),
                Description = "Updated block"
            };

            var updateResponse = await Client.PatchAsJsonAsync($"{_baseUrl}/{created.Id}", updateDto);
            updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // DELETE
            var deleteResponse = await Client.DeleteAsync($"{_baseUrl}/{created.Id}");
            deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // Verify deletion
            var finalGetResponse = await Client.GetAsync(_baseUrl);
            finalGetResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var finalList = await finalGetResponse.Content.ReadFromJsonAsync<List<BookingRuleBlockedPeriodDto>>();
            finalList.Should().NotContain(bp => bp.Id == created.Id);
        }
    }
}
