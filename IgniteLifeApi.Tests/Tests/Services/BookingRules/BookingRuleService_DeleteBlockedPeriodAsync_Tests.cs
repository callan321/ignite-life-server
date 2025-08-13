using FluentAssertions;
using IgniteLifeApi.Application.Services.Common;
using IgniteLifeApi.Application.Services.Implementations;
using IgniteLifeApi.Domain.Entities;
using IgniteLifeApi.Tests.Infrastructure;
using IgniteLifeApi.Tests.Seeders;
using Microsoft.EntityFrameworkCore;

namespace IgniteLifeApi.Tests.Services.BookingRules
{
    public class BookingRuleService_DeleteBlockedPeriodAsync_Tests
    {
        [Fact]
        public async Task ShouldReturnNotFound_WhenNoBookingRulesExist()
        {
            using var dbFactory = new InMemoryDbFactory();
            var service = new BookingRuleService(dbFactory.Context);

            var result = await service.DeleteBlockedPeriodAsync(Guid.NewGuid());

            result.Status.Should().Be(ServiceResultStatus.NotFound);
        }

        [Fact]
        public async Task ShouldReturnNotFound_WhenBlockedPeriodDoesNotExist()
        {
            using var dbFactory = new InMemoryDbFactory(ctx =>
                BookingRulesTestSeeder.SeedBookingRules(ctx));

            var service = new BookingRuleService(dbFactory.Context);
            var result = await service.DeleteBlockedPeriodAsync(Guid.NewGuid());

            result.Status.Should().Be(ServiceResultStatus.NotFound);
        }

        [Fact]
        public async Task ShouldDeleteBlockedPeriod_WhenExists()
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
                        Description = "To delete"
                    });
                }));

            var existingId = dbFactory.Context.BookingRules.Include(b => b.BlockedPeriods).First().BlockedPeriods.First().Id;

            var service = new BookingRuleService(dbFactory.Context);
            var result = await service.DeleteBlockedPeriodAsync(existingId);

            result.Status.Should().Be(ServiceResultStatus.Success);

            var updatedRules = await dbFactory.Context.BookingRules.Include(br => br.BlockedPeriods).FirstAsync();
            updatedRules.BlockedPeriods.Should().BeEmpty();
        }

        [Fact]
        public async Task ShouldReturnNotFound_WhenBlockedPeriodIdIsEmpty()
        {
            using var dbFactory = new InMemoryDbFactory(ctx =>
                BookingRulesTestSeeder.SeedBookingRules(ctx));

            var service = new BookingRuleService(dbFactory.Context);
            var result = await service.DeleteBlockedPeriodAsync(Guid.Empty);

            result.Status.Should().Be(ServiceResultStatus.NotFound);
        }

        [Fact]
        public async Task ShouldOnlyRemoveSpecifiedBlockedPeriod()
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
                        Description = "Keep"
                    });

                    r.BlockedPeriods.Add(new BookingRuleBlockedPeriod
                    {
                        Id = Guid.NewGuid(),
                        BookingRulesId = r.Id,
                        StartDateTimeUtc = DateTime.UtcNow.AddDays(2),
                        EndDateTimeUtc = DateTime.UtcNow.AddDays(3),
                        Description = "Delete me"
                    });
                }));

            var deleteId = dbFactory.Context.BookingRules.Include(b => b.BlockedPeriods)
                .First().BlockedPeriods.Last().Id;

            var service = new BookingRuleService(dbFactory.Context);
            var result = await service.DeleteBlockedPeriodAsync(deleteId);

            result.Status.Should().Be(ServiceResultStatus.Success);

            var updatedRules = await dbFactory.Context.BookingRules.Include(br => br.BlockedPeriods).FirstAsync();
            updatedRules.BlockedPeriods.Should().ContainSingle(bp => bp.Description == "Keep");
        }
    }
}
