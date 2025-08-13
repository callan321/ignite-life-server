using FluentAssertions;
using IgniteLifeApi.Application.Dtos.BookingRuleBlockedPeriod;
using IgniteLifeApi.Application.Services.Common;
using IgniteLifeApi.Application.Services.Implementations;
using IgniteLifeApi.Domain.Entities;
using IgniteLifeApi.Tests.Infrastructure;
using IgniteLifeApi.Tests.Seeders;

namespace IgniteLifeApi.Tests.Services.BookingRules
{
    public class BookingRuleService_UpdateBlockedPeriodAsync_Tests
    {
        [Fact]
        public async Task ShouldReturnNotFound_WhenNoBookingRulesExist()
        {
            using var dbFactory = new InMemoryDbFactory();
            var service = new BookingRuleService(dbFactory.Context);

            var result = await service.UpdateBlockedPeriodAsync(Guid.NewGuid(), new UpdateBookingRuleBlockedPeriodDto());

            result.Status.Should().Be(ServiceResultStatus.NotFound);
        }

        [Fact]
        public async Task ShouldReturnNotFound_WhenBlockedPeriodDoesNotExist()
        {
            using var dbFactory = new InMemoryDbFactory(ctx =>
                BookingRulesTestSeeder.SeedBookingRules(ctx));

            var service = new BookingRuleService(dbFactory.Context);
            var result = await service.UpdateBlockedPeriodAsync(Guid.NewGuid(), new UpdateBookingRuleBlockedPeriodDto());

            result.Status.Should().Be(ServiceResultStatus.NotFound);
        }

        [Fact]
        public async Task ShouldUpdateBlockedPeriod_WhenValid()
        {
            var newId = Guid.NewGuid();
            using var dbFactory = new InMemoryDbFactory(ctx =>
                BookingRulesTestSeeder.SeedBookingRules(ctx, r =>
                {
                    r.BlockedPeriods.Add(new BookingRuleBlockedPeriod
                    {
                        Id = newId,
                        BookingRulesId = r.Id,
                        StartDateTimeUtc = DateTime.UtcNow,
                        EndDateTimeUtc = DateTime.UtcNow.AddDays(1),
                        Description = "Old Desc"
                    });
                }));

            var dto = new UpdateBookingRuleBlockedPeriodDto
            {
                StartDateTimeUtc = DateTime.UtcNow.AddDays(2),
                EndDateTimeUtc = DateTime.UtcNow.AddDays(3),
                Description = "Updated Desc"
            };

            var service = new BookingRuleService(dbFactory.Context);
            var result = await service.UpdateBlockedPeriodAsync(newId, dto);

            result.Status.Should().Be(ServiceResultStatus.Success);

            var updated = await dbFactory.Context.BookingRuleBlockedPeriods.FindAsync(newId);
            updated!.Description.Should().Be("Updated Desc");
            updated.StartDateTimeUtc.Should().Be(dto.StartDateTimeUtc);
            updated.EndDateTimeUtc.Should().Be(dto.EndDateTimeUtc);
        }

        [Fact]
        public async Task ShouldTrimDescription_WhenExtraSpaces()
        {
            var newId = Guid.NewGuid();
            using var dbFactory = new InMemoryDbFactory(ctx =>
                BookingRulesTestSeeder.SeedBookingRules(ctx, r =>
                {
                    r.BlockedPeriods.Add(new BookingRuleBlockedPeriod
                    {
                        Id = newId,
                        BookingRulesId = r.Id,
                        StartDateTimeUtc = DateTime.UtcNow,
                        EndDateTimeUtc = DateTime.UtcNow.AddDays(1),
                        Description = "Something"
                    });
                }));

            var dto = new UpdateBookingRuleBlockedPeriodDto
            {
                Description = "  Trimmed Desc  "
            };

            var service = new BookingRuleService(dbFactory.Context);
            var result = await service.UpdateBlockedPeriodAsync(newId, dto);

            result.Status.Should().Be(ServiceResultStatus.Success);

            var updated = await dbFactory.Context.BookingRuleBlockedPeriods.FindAsync(newId);
            updated!.Description.Should().Be("Trimmed Desc");
        }
    }
}
