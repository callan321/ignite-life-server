using FluentAssertions;
using IgniteLifeApi.Api.Controllers;
using IgniteLifeApi.Application.Dtos.BookingRuleBlockedPeriod;
using IgniteLifeApi.Tests.TestInfrastructure;
using IgniteLifeApi.Tests.Utilities;
using Microsoft.AspNetCore.Http.HttpResults;
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
        public async Task PatchBlockedPeriod_ShouldUpdateSuccessfully()
        {
            // Arrange
            var dto = new CreateBookingRuleBlockedPeriodDto
            {
                StartDateTime = DateTime.UtcNow.AddDays(700),
                EndDateTime = DateTime.UtcNow.AddDays(702),
                Description = "Patch Test Block"
            };

            // CREATE
            var create = await _client.PostAsJsonAsync(_baseUrl, dto);
            create.StatusCode.Should().Be(HttpStatusCode.Created);

            var created = await create.Content.ReadFromJsonAsync<BookingRuleBlockedPeriodDto>();
            created.Should().NotBeNull();

            // PATCH
            var updateDto = new UpdateBookingRuleBlockedPeriodDto
            {
                StartDateTime = created.StartDateTime.AddDays(1),
                EndDateTime = created.EndDateTime.AddDays(1),
                Description = "Updated Block"
            };

            var patch = await _client.PatchAsJsonAsync($"{_baseUrl}/{created.Id}", updateDto);
            patch.StatusCode.Should().Be(HttpStatusCode.OK);

            var updated = await patch.Content.ReadFromJsonAsync<BookingRuleBlockedPeriodDto>();
            updated.Should().NotBeNull();

            // Ensure values changed
            updated.Description.Should().Be("Updated Block");
            updated.Description.Should().NotBe(created.Description);

            updated.StartDateTime.Should().BeCloseTo((DateTime)updateDto.StartDateTime, TimeSpan.FromSeconds(1));
            updated.StartDateTime.Should().NotBe(created.StartDateTime);

            updated.EndDateTime.Should().BeCloseTo((DateTime)updateDto.EndDateTime, TimeSpan.FromSeconds(1));
            updated.EndDateTime.Should().NotBe(created.EndDateTime);

            // CLEANUP
            var delete = await _client.DeleteAsync($"{_baseUrl}/{updated.Id}");
            delete.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task InvalidDtos_ShouldReturnBadRequest()
        {
            // Arrange
            string Description = "Invalid Block";
            // Act
            var response = await _client.PostAsJsonAsync(_baseUrl, Description);
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task OverlappingBlockedPeriods_ShouldReturnConflict()
        {
            // Arrange
            var dto1 = new CreateBookingRuleBlockedPeriodDto
            {
                StartDateTime = DateTime.UtcNow.AddDays(800),
                EndDateTime = DateTime.UtcNow.AddDays(802),
                Description = "First Block"
            };
            var dto2 = new CreateBookingRuleBlockedPeriodDto
            {
                StartDateTime = DateTime.UtcNow.AddDays(801),
                EndDateTime = DateTime.UtcNow.AddDays(803),
                Description = "Overlapping Block"
            };
            // CREATE first blocked period
            var create1 = await _client.PostAsJsonAsync(_baseUrl, dto1);
            create1.StatusCode.Should().Be(HttpStatusCode.Created);
            // Attempt to create overlapping blocked period
            var create2 = await _client.PostAsJsonAsync(_baseUrl, dto2);
            create2.StatusCode.Should().Be(HttpStatusCode.Conflict);
            // CLEANUP
            var created1 = await create1.Content.ReadFromJsonAsync<BookingRuleBlockedPeriodDto>();
            created1.Should().NotBeNull();
            var delete = await _client.DeleteAsync($"{_baseUrl}/{created1.Id}");
            delete.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task DeleteBlockedPeriod_ShouldReturnNotFound_WhenIdDoesNotExist()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();
            // Act
            var response = await _client.DeleteAsync($"{_baseUrl}/{nonExistentId}");
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task CreateBlockedPeriod_ShouldReturnBadRequest_WhenDtoIsNull()
        {
            // Arrange
            CreateBookingRuleBlockedPeriodDto? dto = null;
            // Act
            var response = await _client.PostAsJsonAsync(_baseUrl, dto);
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateBlockedPeriod_ShouldReturnBadRequest_WhenStartTimeIsAfterEndTime()
        {
            // Arrange
            var dto = new CreateBookingRuleBlockedPeriodDto
            {
                StartDateTime = DateTime.UtcNow.AddDays(10),
                EndDateTime = DateTime.UtcNow.AddDays(5),
                Description = "Invalid Block"
            };
            // Act
            var response = await _client.PostAsJsonAsync(_baseUrl, dto);
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task UpdateBlockedPeriod_ShouldReturnNotFound_WhenIdDoesNotExist()
        {
            // Arrange
            var dto = new UpdateBookingRuleBlockedPeriodDto
            {
                StartDateTime = DateTime.UtcNow.AddDays(10),
                EndDateTime = DateTime.UtcNow.AddDays(12),
                Description = "Non-existent Block"
            };
            // Act
            var response = await _client.PatchAsJsonAsync($"{_baseUrl}/{Guid.NewGuid()}", dto);
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task UpdateBlockedPeriod_ShouldReturnBadRequest_WhenDtoIsNull()
        {
            // Arrange
            UpdateBookingRuleBlockedPeriodDto? dto = null;
            // Act
            var response = await _client.PatchAsJsonAsync($"{_baseUrl}/some-id", dto);
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task UpdateBlockedPeriod_ShouldReturnConflict_WhenOverlappingWithExistingPeriod()
        {
            // Arrange
            var createDto = new CreateBookingRuleBlockedPeriodDto
            {
                StartDateTime = DateTime.UtcNow.AddDays(900),
                EndDateTime = DateTime.UtcNow.AddDays(902),
                Description = "Original Block"
            };
            var overlapDto = new CreateBookingRuleBlockedPeriodDto
            {
                StartDateTime = DateTime.UtcNow.AddDays(910),
                EndDateTime = DateTime.UtcNow.AddDays(920),
                Description = "Original Block"
            };

            // CREATE original blocked period
            var createResponse = await _client.PostAsJsonAsync(_baseUrl, createDto);
            createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
            var created = await createResponse.Content.ReadFromJsonAsync<BookingRuleBlockedPeriodDto>();
            created.Should().NotBeNull();

            // CREATE overlapping blocked period
            var blockResponse = await _client.PostAsJsonAsync(_baseUrl, overlapDto);
            blockResponse.StatusCode.Should().Be(HttpStatusCode.Created);

            // Attempt to update with overlapping period
            var updateDto = new UpdateBookingRuleBlockedPeriodDto
            {
                StartDateTime = DateTime.UtcNow.AddDays(911),
                EndDateTime = DateTime.UtcNow.AddDays(912),
                Description = "Overlapping Update Block"
            };
            var updateResponse = await _client.PatchAsJsonAsync($"{_baseUrl}/{created.Id}", updateDto);
            updateResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);
            // CLEANUP
            var deleteResponse = await _client.DeleteAsync($"{_baseUrl}/{created.Id}");
            deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }


        [Fact]
        public async Task PatchBlockedPeriod_ShouldUpdateOnlyDescription()
        {
            // Arrange
            var dto = new CreateBookingRuleBlockedPeriodDto
            {
                StartDateTime = DateTime.UtcNow.AddDays(1000),
                EndDateTime = DateTime.UtcNow.AddDays(1002),
                Description = "Initial Desc"
            };

            var create = await _client.PostAsJsonAsync(_baseUrl, dto);
            create.StatusCode.Should().Be(HttpStatusCode.Created);

            var created = await create.Content.ReadFromJsonAsync<BookingRuleBlockedPeriodDto>();
            created.Should().NotBeNull();

            // PATCH only description
            var updateDto = new UpdateBookingRuleBlockedPeriodDto
            {
                Description = "Updated Desc"
            };

            var patch = await _client.PatchAsJsonAsync($"{_baseUrl}/{created.Id}", updateDto);
            patch.StatusCode.Should().Be(HttpStatusCode.OK);

            var updated = await patch.Content.ReadFromJsonAsync<BookingRuleBlockedPeriodDto>();
            updated.Should().NotBeNull();
            updated.Description.Should().Be("Updated Desc");
            updated.StartDateTime.Should().BeCloseTo(created.StartDateTime, TimeSpan.FromSeconds(1));
            updated.EndDateTime.Should().BeCloseTo(created.EndDateTime, TimeSpan.FromSeconds(1));

            // CLEANUP
            var delete = await _client.DeleteAsync($"{_baseUrl}/{updated.Id}");
            delete.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task GetAllBlockedPeriods_ShouldReturnInAscendingOrder()
        {
            // Arrange
            var dto1 = new CreateBookingRuleBlockedPeriodDto
            {
                StartDateTime = DateTime.UtcNow.AddDays(2000),
                EndDateTime = DateTime.UtcNow.AddDays(2002),
                Description = "Earlier"
            };
            var dto2 = new CreateBookingRuleBlockedPeriodDto
            {
                StartDateTime = DateTime.UtcNow.AddDays(2010),
                EndDateTime = DateTime.UtcNow.AddDays(2012),
                Description = "Later"
            };

            var create1 = await _client.PostAsJsonAsync(_baseUrl, dto1);
            create1.StatusCode.Should().Be(HttpStatusCode.Created);
            var created1 = await create1.Content.ReadFromJsonAsync<BookingRuleBlockedPeriodDto>();
            created1.Should().NotBeNull();

            var create2 = await _client.PostAsJsonAsync(_baseUrl, dto2);
            create2.StatusCode.Should().Be(HttpStatusCode.Created);
            var created2 = await create2.Content.ReadFromJsonAsync<BookingRuleBlockedPeriodDto>();
            created2.Should().NotBeNull();

            // Act
            var response = await _client.GetAsync(_baseUrl);
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var periods = await response.Content.ReadFromJsonAsync<List<BookingRuleBlockedPeriodDto>>();
            periods.Should().NotBeNull();
            periods.Should().Contain(p => p.Id == created1.Id);
            periods.Should().Contain(p => p.Id == created2.Id);

            // Verify ascending order
            periods.OrderBy(p => p.StartDateTime).Should().ContainInOrder(periods);

            // CLEANUP
            await _client.DeleteAsync($"{_baseUrl}/{created1.Id}");
            await _client.DeleteAsync($"{_baseUrl}/{created2.Id}");
        }

        [Fact]
        public async Task CreateBlockedPeriods_TouchingButNotOverlapping_ShouldSucceed()
        {
            // Arrange
            var start = DateTime.UtcNow.AddDays(3000);
            var dto1 = new CreateBookingRuleBlockedPeriodDto
            {
                StartDateTime = start,
                EndDateTime = start.AddDays(1),
                Description = "First"
            };
            var dto2 = new CreateBookingRuleBlockedPeriodDto
            {
                StartDateTime = start.AddDays(1),
                EndDateTime = start.AddDays(2),
                Description = "Second"
            };

            var create1 = await _client.PostAsJsonAsync(_baseUrl, dto1);
            create1.StatusCode.Should().Be(HttpStatusCode.Created);
            var created1 = await create1.Content.ReadFromJsonAsync<BookingRuleBlockedPeriodDto>();
            created1.Should().NotBeNull();

            var create2 = await _client.PostAsJsonAsync(_baseUrl, dto2);
            create2.StatusCode.Should().Be(HttpStatusCode.Created);
            var created2 = await create2.Content.ReadFromJsonAsync<BookingRuleBlockedPeriodDto>();
            created2.Should().NotBeNull();

            // CLEANUP
            await _client.DeleteAsync($"{_baseUrl}/{created1.Id}");
            await _client.DeleteAsync($"{_baseUrl}/{created2.Id}");
        }

        [Fact]
        public async Task CreateAndUpdateBlockedPeriod_ShouldAllowMissingDescription()
        {
            // CREATE without description
            var dto = new CreateBookingRuleBlockedPeriodDto
            {
                StartDateTime = DateTime.UtcNow.AddDays(4000),
                EndDateTime = DateTime.UtcNow.AddDays(4002),
                Description = null
            };

            var create = await _client.PostAsJsonAsync(_baseUrl, dto);
            create.StatusCode.Should().Be(HttpStatusCode.Created);

            var created = await create.Content.ReadFromJsonAsync<BookingRuleBlockedPeriodDto>();
            created.Should().NotBeNull();
            created.Description.Should().BeNull();

            // UPDATE without description
            var updateDto = new UpdateBookingRuleBlockedPeriodDto
            {
                StartDateTime = created.StartDateTime.AddHours(1),
                EndDateTime = created.EndDateTime.AddHours(1),
                Description = null
            };

            var patch = await _client.PatchAsJsonAsync($"{_baseUrl}/{created.Id}", updateDto);
            patch.StatusCode.Should().Be(HttpStatusCode.OK);

            var updated = await patch.Content.ReadFromJsonAsync<BookingRuleBlockedPeriodDto>();
            updated.Should().NotBeNull();
            updated.Description.Should().BeNull();

            // CLEANUP
            var delete = await _client.DeleteAsync($"{_baseUrl}/{updated.Id}");
            delete.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task UpdateBlockedPeriod_ShouldReturnBadRequest_WhenIdMismatch()
        {
            // Arrange
            var dto = new UpdateBookingRuleBlockedPeriodDto
            {
                StartDateTime = DateTime.UtcNow.AddDays(255),
                EndDateTime = DateTime.UtcNow.AddDays(256),
                Description = "Test mismatch"
            };

            // Act
            var response = await _client.PatchAsJsonAsync($"{_baseUrl}/{Guid.NewGuid()}", dto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task CreateBlockedPeriod_ShouldReturnBadRequest_WhenEmptyJson()
        {
            var content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");
            var response = await _client.PostAsync(_baseUrl, content);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateBlockedPeriod_ShouldReturnUnsupportedMediaType_WhenInvalidContentType()
        {
            var content = new StringContent("<xml></xml>", System.Text.Encoding.UTF8, "application/xml");
            var response = await _client.PostAsync(_baseUrl, content);
            response.StatusCode.Should().Be(HttpStatusCode.UnsupportedMediaType);
        }

        [Fact]
        public async Task CreateBlockedPeriod_ShouldReturnBadRequest_WhenDescriptionExceedsMaxLength()
        {
            // Arrange
            var dto = new CreateBookingRuleBlockedPeriodDto
            {
                StartDateTime = DateTime.UtcNow.AddDays(300),
                EndDateTime = DateTime.UtcNow.AddDays(302),
                Description = new string('X', 10000)
            };

            // Act
            var response = await _client.PostAsJsonAsync(_baseUrl, dto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateBlockedPeriod_ShouldReturnBadRequest_WhenStartTimeIsMissing()
        {
            // Arrange
            var dto = new
            {
                // StartDateTime is intentionally missing
                EndDateTime = DateTime.UtcNow.AddDays(450),
                Description = "Missing Start Time"
            };

            // Act
            var response = await _client.PostAsJsonAsync(_baseUrl, dto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateBlockedPeriod_ShouldReturnBadRequest_WhenEndTimeIsMissing()
        {
            // Arrange
            var dto = new
            {
                StartDateTime = DateTime.UtcNow.AddDays(455),
                // EndDateTime is intentionally missing
                Description = "Missing End Time"
            };

            // Act
            var response = await _client.PostAsJsonAsync(_baseUrl, dto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task PatchBlockedPeriod_WithNoChanges_ShouldReturnSameData()
        {
            // Arrange - Create a new blocked period
            var dto = new CreateBookingRuleBlockedPeriodDto
            {
                StartDateTime = DateTime.UtcNow.AddDays(20),
                EndDateTime = DateTime.UtcNow.AddDays(21),
                Description = "No Changes"
            };
            var create = await _client.PostAsJsonAsync(_baseUrl, dto);
            var created = await create.Content.ReadFromJsonAsync<BookingRuleBlockedPeriodDto>();
            created.Should().NotBeNull();

            // Act - Send a PATCH request with no actual changes
            var updateDto = new UpdateBookingRuleBlockedPeriodDto
            {
                StartDateTime = created.StartDateTime,
                EndDateTime = created.EndDateTime,
                Description = created.Description
            };
            var patch = await _client.PatchAsJsonAsync($"{_baseUrl}/{created.Id}", updateDto);
            patch.StatusCode.Should().Be(HttpStatusCode.OK);

            var updated = await patch.Content.ReadFromJsonAsync<BookingRuleBlockedPeriodDto>();

            // Assert - Ensure the returned object is the same as the original
            updated.Should().BeEquivalentTo(created);

            // Cleanup - Delete the test data
            await _client.DeleteAsync($"{_baseUrl}/{created.Id}");
        }

        [Fact]
        public async Task DeletedBlockedPeriod_ShouldNotAppearInGetAll()
        {
            // Arrange - Create a new blocked period
            var dto = new CreateBookingRuleBlockedPeriodDto
            {
                StartDateTime = DateTime.UtcNow.AddDays(10),
                EndDateTime = DateTime.UtcNow.AddDays(11),
                Description = "Delete Test"
            };
            var create = await _client.PostAsJsonAsync(_baseUrl, dto);
            var created = await create.Content.ReadFromJsonAsync<BookingRuleBlockedPeriodDto>();
            created.Should().NotBeNull();

            // Act - Delete the created blocked period
            var delete = await _client.DeleteAsync($"{_baseUrl}/{created!.Id}");
            delete.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // Assert - Fetch all blocked periods and ensure the deleted one is not present
            var get = await _client.GetAsync(_baseUrl);
            var periods = await get.Content.ReadFromJsonAsync<List<BookingRuleBlockedPeriodDto>>();
            periods.Should().NotContain(p => p.Id == created.Id);
        }

        [Fact]
        public async Task CreateBlockedPeriod_ShouldReturnConflict_WhenDatesAreIdentical()
        {
            // Arrange
            var start = DateTime.UtcNow.AddDays(6000);
            var end = start.AddDays(1);

            var dto = new CreateBookingRuleBlockedPeriodDto
            {
                StartDateTime = start,
                EndDateTime = end,
                Description = "Original Period"
            };

            // Act - create the first one
            var create1 = await _client.PostAsJsonAsync(_baseUrl, dto);
            create1.StatusCode.Should().Be(HttpStatusCode.Created);
            var created1 = await create1.Content.ReadFromJsonAsync<BookingRuleBlockedPeriodDto>();
            created1.Should().NotBeNull();

            // Act - attempt to create a duplicate
            var create2 = await _client.PostAsJsonAsync(_baseUrl, dto);

            // Assert - second one should fail with 409 Conflict
            create2.StatusCode.Should().Be(HttpStatusCode.Conflict);

            // Cleanup
            var delete = await _client.DeleteAsync($"{_baseUrl}/{created1!.Id}");
            delete.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task ConcurrentCreates_ShouldAllowOnlyOne_WhenPeriodsOverlap()
        {
            var start = DateTime.UtcNow.AddDays(900);
            var dto1 = new CreateBookingRuleBlockedPeriodDto
            {
                StartDateTime = start,
                EndDateTime = start.AddDays(4),
                Description = "Concurrent Block 1"
            };
            var dto2 = new CreateBookingRuleBlockedPeriodDto
            {
                StartDateTime = start.AddDays(1),
                EndDateTime = start.AddDays(3),
                Description = "Concurrent Block 2"
            };

            // Start both requests concurrently
            var task1 = _client.PostAsJsonAsync(_baseUrl, dto1);
            var task2 = _client.PostAsJsonAsync(_baseUrl, dto2);

            var responses = await Task.WhenAll(task1, task2);

            responses.Count(r => r.StatusCode == HttpStatusCode.Created).Should().Be(1);
            responses.Count(r => r.StatusCode == HttpStatusCode.Conflict).Should().Be(1);

            // Cleanup
            var createdResponse = responses.First(r => r.StatusCode == HttpStatusCode.Created);
            var created = await createdResponse.Content.ReadFromJsonAsync<BookingRuleBlockedPeriodDto>();

            await _client.DeleteAsync($"{_baseUrl}/{created!.Id}");
        }

        [Fact]
        public async Task UpdateBlockedPeriod_ShouldReturnBadRequest_WhenEndTimeBeforeStartTime()
        {
            // Arrange - create a valid blocked period first
            var createDto = new CreateBookingRuleBlockedPeriodDto
            {
                StartDateTime = DateTime.UtcNow.AddDays(50),
                EndDateTime = DateTime.UtcNow.AddDays(55),
                Description = "Invalid Update Test"
            };

            var createResponse = await _client.PostAsJsonAsync(_baseUrl, createDto);
            createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

            var created = await createResponse.Content.ReadFromJsonAsync<BookingRuleBlockedPeriodDto>();
            created.Should().NotBeNull();

            // Act - attempt to update with EndDateTime before StartDateTime
            var updateDto = new UpdateBookingRuleBlockedPeriodDto
            {
                EndDateTime = DateTime.UtcNow.AddDays(49),
                Description = created.Description
            };

            var patchResponse = await _client.PatchAsJsonAsync($"{_baseUrl}/{created.Id}", updateDto);

            // Assert
            patchResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            // Cleanup
            var deleteResponse = await _client.DeleteAsync($"{_baseUrl}/{created.Id}");
            deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }
    }
}