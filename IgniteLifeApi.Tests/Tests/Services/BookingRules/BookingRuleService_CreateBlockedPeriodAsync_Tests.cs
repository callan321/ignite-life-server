using FluentAssertions;
using IgniteLifeApi.Application.Dtos.BookingRuleBlockedPeriod;
using IgniteLifeApi.Application.Services.Common;
using IgniteLifeApi.Application.Services.Implementations;
using IgniteLifeApi.Domain.Entities;
using IgniteLifeApi.Tests.Infrastructure;
using IgniteLifeApi.Tests.Seeders;

namespace IgniteLifeApi.Tests.Tests.Services.BookingRules
{
    public class BookingRuleService_CreateBlockedPeriodAsync_Tests
    {
        [Fact]
        public async Task ShouldReturnConflict_WhenOverlappingPeriodExists()
        {
            using var dbFactory = new InMemoryDbFactory(ctx =>
                BookingRulesTestSeeder.SeedBookingRules(ctx, rules =>
                {
                    rules.BlockedPeriods.Add(new BookingRuleBlockedPeriod
                    {
                        Id = Guid.NewGuid(),
                        BookingRulesId = rules.Id,
                        StartDateTimeUtc = DateTime.UtcNow,
                        EndDateTimeUtc = DateTime.UtcNow.AddDays(2)
                    });
                }));

            var service = new BookingRuleService(dbFactory.Context);

            var dto = new CreateBookingRuleBlockedPeriodDto
            {
                StartDateTimeUtc = DateTime.UtcNow.AddDays(1),
                EndDateTimeUtc = DateTime.UtcNow.AddDays(3),
                Description = "Overlap"
            };

            var result = await service.CreateBlockedPeriodAsync(dto);

            result.Status.Should().Be(ServiceResultStatus.Conflict);
            result.Message.Should().Contain("overlaps");
        }

        [Fact]
        public async Task ShouldTrimDescription_WhenHasExtraSpaces()
        {
            using var dbFactory = new InMemoryDbFactory(ctx =>
                BookingRulesTestSeeder.SeedBookingRules(ctx));

            var service = new BookingRuleService(dbFactory.Context);
            var dto = new CreateBookingRuleBlockedPeriodDto
            {
                StartDateTimeUtc = DateTime.UtcNow.AddDays(5),
                EndDateTimeUtc = DateTime.UtcNow.AddDays(6),
                Description = "  Trim me  "
            };

            var result = await service.CreateBlockedPeriodAsync(dto);

            result.Status.Should().Be(ServiceResultStatus.Created);
            result.Data!.BlockedPeriods.Should().ContainSingle(bp => bp.Description == "Trim me");
        }

        [Fact]
        public async Task ShouldSetDescriptionNull_WhenWhitespaceOnly()
        {
            using var dbFactory = new InMemoryDbFactory(ctx =>
                BookingRulesTestSeeder.SeedBookingRules(ctx));

            var service = new BookingRuleService(dbFactory.Context);
            var dto = new CreateBookingRuleBlockedPeriodDto
            {
                StartDateTimeUtc = DateTime.UtcNow.AddDays(7),
                EndDateTimeUtc = DateTime.UtcNow.AddDays(8),
                Description = "   "
            };

            var result = await service.CreateBlockedPeriodAsync(dto);

            result.Status.Should().Be(ServiceResultStatus.Created);
            result.Data!.BlockedPeriods.Should().ContainSingle(bp => bp.Description == null);
        }

        [Fact]
        public async Task ShouldCreateSuccessfully_WhenValid()
        {
            using var dbFactory = new InMemoryDbFactory(ctx =>
                BookingRulesTestSeeder.SeedBookingRules(ctx));

            var service = new BookingRuleService(dbFactory.Context);
            var dto = new CreateBookingRuleBlockedPeriodDto
            {
                StartDateTimeUtc = DateTime.UtcNow.AddDays(10),
                EndDateTimeUtc = DateTime.UtcNow.AddDays(11),
                Description = "Valid"
            };

            var result = await service.CreateBlockedPeriodAsync(dto);

            result.Status.Should().Be(ServiceResultStatus.Created);
            result.Data!.BlockedPeriods.Should().ContainSingle(bp =>
                bp.StartDateTimeUtc == dto.StartDateTimeUtc &&
                bp.EndDateTimeUtc == dto.EndDateTimeUtc &&
                bp.Description == "Valid");
        }

        [Fact]
        public async Task ShouldPass_WhenDescriptionAtMaxLength()
        {
            using var dbFactory = new InMemoryDbFactory(ctx =>
                BookingRulesTestSeeder.SeedBookingRules(ctx));

            var maxDesc = new string('a', Domain.Constants.FieldLengths.Description);

            var service = new BookingRuleService(dbFactory.Context);
            var dto = new CreateBookingRuleBlockedPeriodDto
            {
                StartDateTimeUtc = DateTime.UtcNow.AddDays(5),
                EndDateTimeUtc = DateTime.UtcNow.AddDays(6),
                Description = maxDesc
            };

            var result = await service.CreateBlockedPeriodAsync(dto);

            result.Status.Should().Be(ServiceResultStatus.Created);
            result.Data!.BlockedPeriods.Should().ContainSingle(bp => bp.Description == maxDesc);
        }

        [Fact]
        public async Task ShouldFail_WhenNoBookingRulesSeeded()
        {
            // No seed passed → DB is empty
            using var dbFactory = new InMemoryDbFactory();

            var service = new BookingRuleService(dbFactory.Context);
            var dto = new CreateBookingRuleBlockedPeriodDto
            {
                StartDateTimeUtc = DateTime.UtcNow.AddDays(5),
                EndDateTimeUtc = DateTime.UtcNow.AddDays(6),
                Description = "No rules"
            };

            var result = await service.CreateBlockedPeriodAsync(dto);

            result.Status.Should().Be(ServiceResultStatus.NotFound);
        }

        [Fact]
        public async Task ShouldRespectOverlapRules_WhenTouchingEdges()
        {
            using var dbFactory = new InMemoryDbFactory(ctx =>
                BookingRulesTestSeeder.SeedBookingRules(ctx, rules =>
                {
                    rules.BlockedPeriods.Add(new BookingRuleBlockedPeriod
                    {
                        Id = Guid.NewGuid(),
                        BookingRulesId = rules.Id,
                        StartDateTimeUtc = DateTime.UtcNow,
                        EndDateTimeUtc = DateTime.UtcNow.AddDays(2)
                    });
                }));

            var service = new BookingRuleService(dbFactory.Context);

            // This starts exactly when the existing one ends
            var dto = new CreateBookingRuleBlockedPeriodDto
            {
                StartDateTimeUtc = DateTime.UtcNow.AddDays(2),
                EndDateTimeUtc = DateTime.UtcNow.AddDays(3),
                Description = "Edge touch"
            };

            var result = await service.CreateBlockedPeriodAsync(dto);

            // Depending on business rules, adjust this assertion
            result.Status.Should().Be(ServiceResultStatus.Created);
        }
    }
}
