using FluentAssertions;
using IgniteLifeApi.Application.Dtos.BookingRuleBlockedPeriod;
using IgniteLifeApi.Application.Dtos.BookingRules;
using IgniteLifeApi.Tests.Infrastructure;
using IgniteLifeApi.Tests.Seeders;
using System.Net;
using System.Net.Http.Json;

namespace IgniteLifeApi.Tests.Tests.Controllers;

public class BookingRuleBlockedPeriodsApiTests
{
    private const string RulesUrl = "/api/booking-rules";
    private const string BlockedPeriodsUrl = "/api/booking-rules/blocked-periods";

    private static ApiPostgresTestApplicationFactory CreateFactory()
        => new ApiPostgresTestApplicationFactory();

    // ---------- AUTHORIZATION ----------

    [Theory]
    [InlineData(nameof(ApiPostgresTestApplicationFactory.CreateAnonymousClient), HttpStatusCode.Unauthorized)]
    [InlineData(nameof(ApiPostgresTestApplicationFactory.CreateVerifiedClient), HttpStatusCode.Forbidden)]
    [InlineData(nameof(ApiPostgresTestApplicationFactory.CreateAuthenticatedButUnverifiedClient), HttpStatusCode.Forbidden)]
    public async Task NonAdminClients_ShouldBeBlocked(string clientFactoryMethod, HttpStatusCode expectedStatus)
    {
        await using var factory = CreateFactory();
        var method = typeof(ApiPostgresTestApplicationFactory).GetMethod(clientFactoryMethod)!;
        var client = (HttpClient)method.Invoke(factory, null)!;

        var response = await client.GetAsync(RulesUrl);
        response.StatusCode.Should().Be(expectedStatus);
    }

    [Fact]
    public async Task AdminClient_ShouldAccessProtectedEndpoints()
    {
        await using var factory = CreateFactory();
        var client = factory.CreateAdminClient();

        var response = await client.GetAsync(RulesUrl);
        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized)
            .And.NotBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task NonAdminClient_ShouldBeForbidden_OnUpdateAndDelete()
    {
        await using var factory = CreateFactory();
        var client = factory.CreateVerifiedClient(); // not admin

        var updateResp = await client.PatchAsJsonAsync($"{BlockedPeriodsUrl}/{Guid.NewGuid()}",
            new UpdateBookingRuleBlockedPeriodDto
            {
                StartDateTimeUtc = DateTime.UtcNow.AddDays(1),
                EndDateTimeUtc = DateTime.UtcNow.AddDays(2),
                Description = "Hacker Attempt"
            });
        updateResp.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        var deleteResp = await client.DeleteAsync($"{BlockedPeriodsUrl}/{Guid.NewGuid()}");
        deleteResp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // ---------- GET booking rules ----------

    [Fact]
    public async Task GetBookingRules_ShouldReturnPreSeededData()
    {
        await using var factory = CreateFactory();
        var client = factory.CreateAdminClient();

        var rules = await client.GetFromJsonAsync<BookingRulesDto>(RulesUrl);

        rules.Should().NotBeNull();
        rules!.Id.Should().Be(BookingRulesTestSeeder.DefaultBookingRulesId);
        rules.OpeningHours.Should().HaveCount(7);
        rules.BlockedPeriods.Should().BeEmpty();
    }

    // ---------- POST blocked period ----------

    [Fact]
    public async Task CreateBlockedPeriod_ShouldAddNewBlockedPeriod()
    {
        await using var factory = CreateFactory();
        var client = factory.CreateAdminClient();

        var dto = new CreateBookingRuleBlockedPeriodDto
        {
            StartDateTimeUtc = DateTime.UtcNow.AddDays(1),
            EndDateTimeUtc = DateTime.UtcNow.AddDays(2),
            Description = "API Created"
        };

        var response = await client.PostAsJsonAsync(BlockedPeriodsUrl, dto);
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var rules = await client.GetFromJsonAsync<BookingRulesDto>(RulesUrl);
        rules!.BlockedPeriods.Should().ContainSingle(bp => bp.Description == "API Created");
    }

    [Fact]
    public async Task CreateBlockedPeriod_ShouldReturnBadRequest_WhenDatesInvalid()
    {
        await using var factory = CreateFactory();
        var client = factory.CreateAdminClient();

        var dto = new CreateBookingRuleBlockedPeriodDto
        {
            StartDateTimeUtc = DateTime.UtcNow.AddDays(2),
            EndDateTimeUtc = DateTime.UtcNow.AddDays(1),
            Description = "Invalid"
        };

        var response = await client.PostAsJsonAsync(BlockedPeriodsUrl, dto);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateBlockedPeriod_ShouldReturnBadRequest_WhenOverlapsExisting()
    {
        await using var factory = CreateFactory();
        var client = factory.CreateAdminClient();

        var dto1 = new CreateBookingRuleBlockedPeriodDto
        {
            StartDateTimeUtc = DateTime.UtcNow.AddDays(1),
            EndDateTimeUtc = DateTime.UtcNow.AddDays(2),
            Description = "First"
        };
        await client.PostAsJsonAsync(BlockedPeriodsUrl, dto1);

        var dto2 = new CreateBookingRuleBlockedPeriodDto
        {
            StartDateTimeUtc = DateTime.UtcNow.AddDays(1).AddHours(12),
            EndDateTimeUtc = DateTime.UtcNow.AddDays(3),
            Description = "Overlaps"
        };
        var response = await client.PostAsJsonAsync(BlockedPeriodsUrl, dto2);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    // ---------- PATCH blocked period ----------

    [Fact]
    public async Task UpdateBlockedPeriod_ShouldModifyExistingPeriod()
    {
        await using var factory = CreateFactory();
        var client = factory.CreateAdminClient();

        var createDto = new CreateBookingRuleBlockedPeriodDto
        {
            StartDateTimeUtc = DateTime.UtcNow.AddDays(1),
            EndDateTimeUtc = DateTime.UtcNow.AddDays(2),
            Description = "Initial"
        };
        var createResp = await client.PostAsJsonAsync(BlockedPeriodsUrl, createDto);
        createResp.EnsureSuccessStatusCode();

        var rules = await client.GetFromJsonAsync<BookingRulesDto>(RulesUrl);
        var periodId = rules!.BlockedPeriods.Single().Id;

        var updateDto = new UpdateBookingRuleBlockedPeriodDto
        {
            StartDateTimeUtc = DateTime.UtcNow.AddDays(3),
            EndDateTimeUtc = DateTime.UtcNow.AddDays(4),
            Description = "Updated"
        };

        var updateResp = await client.PatchAsJsonAsync($"{BlockedPeriodsUrl}/{periodId}", updateDto);
        updateResp.StatusCode.Should().Be(HttpStatusCode.OK);

        var updatedRules = await client.GetFromJsonAsync<BookingRulesDto>(RulesUrl);
        updatedRules!.BlockedPeriods.Should().ContainSingle(bp => bp.Description == "Updated");
    }

    [Fact]
    public async Task UpdateBlockedPeriod_ShouldReturnBadRequest_WhenDatesInvalid()
    {
        await using var factory = CreateFactory();
        var client = factory.CreateAdminClient();

        var createDto = new CreateBookingRuleBlockedPeriodDto
        {
            StartDateTimeUtc = DateTime.UtcNow.AddDays(1),
            EndDateTimeUtc = DateTime.UtcNow.AddDays(2),
            Description = "Initial"
        };
        var createResp = await client.PostAsJsonAsync(BlockedPeriodsUrl, createDto);
        createResp.EnsureSuccessStatusCode();

        var rules = await client.GetFromJsonAsync<BookingRulesDto>(RulesUrl);
        var periodId = rules!.BlockedPeriods.Single().Id;

        var updateDto = new UpdateBookingRuleBlockedPeriodDto
        {
            StartDateTimeUtc = DateTime.UtcNow.AddDays(5),
            EndDateTimeUtc = DateTime.UtcNow.AddDays(4),
            Description = "Invalid"
        };

        var response = await client.PatchAsJsonAsync($"{BlockedPeriodsUrl}/{periodId}", updateDto);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateBlockedPeriod_ShouldReturnNotFound_WhenIdDoesNotExist()
    {
        await using var factory = CreateFactory();
        var client = factory.CreateAdminClient();

        var updateDto = new UpdateBookingRuleBlockedPeriodDto
        {
            StartDateTimeUtc = DateTime.UtcNow.AddDays(3),
            EndDateTimeUtc = DateTime.UtcNow.AddDays(4),
            Description = "Non-existent"
        };

        var response = await client.PatchAsJsonAsync($"{BlockedPeriodsUrl}/{Guid.NewGuid()}", updateDto);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }


    // ---------- DELETE blocked period ----------

    [Fact]
    public async Task DeleteBlockedPeriod_ShouldRemoveItFromRules()
    {
        await using var factory = CreateFactory();
        var client = factory.CreateAdminClient();

        var dto = new CreateBookingRuleBlockedPeriodDto
        {
            StartDateTimeUtc = DateTime.UtcNow.AddDays(1),
            EndDateTimeUtc = DateTime.UtcNow.AddDays(2),
            Description = "To Delete"
        };
        var createResp = await client.PostAsJsonAsync(BlockedPeriodsUrl, dto);
        createResp.EnsureSuccessStatusCode();

        var rules = await client.GetFromJsonAsync<BookingRulesDto>(RulesUrl);
        var periodId = rules!.BlockedPeriods.Single().Id;

        var deleteResp = await client.DeleteAsync($"{BlockedPeriodsUrl}/{periodId}");
        deleteResp.StatusCode.Should().Be(HttpStatusCode.OK);

        var rulesAfter = await client.GetFromJsonAsync<BookingRulesDto>(RulesUrl);
        rulesAfter!.BlockedPeriods.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteBlockedPeriod_ShouldReturnNotFound_WhenIdDoesNotExist()
    {
        await using var factory = CreateFactory();
        var client = factory.CreateAdminClient();

        var response = await client.DeleteAsync($"{BlockedPeriodsUrl}/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
