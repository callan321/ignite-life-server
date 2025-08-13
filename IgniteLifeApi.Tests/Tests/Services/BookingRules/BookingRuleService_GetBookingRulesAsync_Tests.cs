using FluentAssertions;
using IgniteLifeApi.Application.Services.Common;
using IgniteLifeApi.Application.Services.Implementations;
using IgniteLifeApi.Domain.Entities;
using IgniteLifeApi.Tests.Infrastructure;
using IgniteLifeApi.Tests.Seeders;
using Microsoft.EntityFrameworkCore;

namespace IgniteLifeApi.Tests.Services.BookingRules
{
    public class BookingRuleService_GetBookingRulesAsync_Tests
    {
        [Fact]
        public async Task ShouldReturnNotFound_WhenNoBookingRulesExist()
        {
            using var dbFactory = new InMemoryDbFactory();
            var service = new BookingRuleService(dbFactory.Context);

            var result = await service.GetBookingRulesAsync();

            result.Status.Should().Be(ServiceResultStatus.NotFound);
            result.Data.Should().BeNull();
        }

        [Fact]
        public async Task ShouldReturnRule_WhenBookingRulesExist()
        {
            using var dbFactory = new InMemoryDbFactory(ctx =>
                BookingRulesTestSeeder.SeedBookingRules(ctx));

            var service = new BookingRuleService(dbFactory.Context);

            var result = await service.GetBookingRulesAsync();

            result.Status.Should().Be(ServiceResultStatus.Success);
            result.Data.Should().NotBeNull();
            result.Data!.Id.Should().Be(BookingRulesTestSeeder.GetBookingRuleGuid());
        }

        [Fact]
        public async Task ShouldIncludeBlockedPeriodsAndOpeningHours_WhenTheyExist()
        {
            using var dbFactory = new InMemoryDbFactory(ctx =>
                BookingRulesTestSeeder.SeedBookingRules(ctx, r =>
                {
                    r.BlockedPeriods.Add(new BookingRuleBlockedPeriod
                    {
                        Id = Guid.NewGuid(),
                        BookingRulesId = r.Id,
                        StartDateTimeUtc = DateTime.UtcNow,
                        EndDateTimeUtc = DateTime.UtcNow.AddDays(1),
                        Description = "Blocked"
                    });
                    var mondayHour = r.OpeningHours.First(oh => oh.DayOfWeek == DayOfWeek.Monday);
                    mondayHour.OpenTimeUtc = new TimeOnly(9, 0);
                    mondayHour.CloseTimeUtc = new TimeOnly(17, 0);
                }));

            var service = new BookingRuleService(dbFactory.Context);

            var result = await service.GetBookingRulesAsync();

            result.Status.Should().Be(ServiceResultStatus.Success);
            result.Data.Should().NotBeNull();

            var rule = result.Data!;
            rule.BlockedPeriods.Should().ContainSingle(bp => bp.Description == "Blocked");
            rule.OpeningHours.Should().ContainSingle(oh => oh.DayOfWeek == DayOfWeek.Monday);
        }

        [Fact]
        public async Task ShouldReturnEmptyCollections_WhenNoBlockedPeriodsOrOpeningHours()
        {
            using var dbFactory = new InMemoryDbFactory(ctx =>
                BookingRulesTestSeeder.SeedBookingRules(ctx, r =>
                {
                    r.BlockedPeriods.Clear();
                    r.OpeningHours.Clear();
                }));

            var service = new BookingRuleService(dbFactory.Context);

            var result = await service.GetBookingRulesAsync();

            result.Status.Should().Be(ServiceResultStatus.Success);
            result.Data.Should().NotBeNull();

            var rule = result.Data!;
            rule.BlockedPeriods.Should().BeEmpty();
            rule.OpeningHours.Should().BeEmpty();
        }
    }
}
