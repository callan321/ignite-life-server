using FluentAssertions;
using IgniteLifeApi.Application.Dtos.BookingRules;
using IgniteLifeApi.Application.Services.Common;
using IgniteLifeApi.Application.Services.Implementations;
using IgniteLifeApi.Domain.Entities;
using IgniteLifeApi.Tests.Infrastructure;
using IgniteLifeApi.Tests.Seeders;
using Microsoft.EntityFrameworkCore;

namespace IgniteLifeApi.Tests.Services.BookingRules
{
    public class BookingRuleService_UpdateBookingRulesAsync_Tests
    {
        [Fact]
        public async Task ShouldReturnNotFound_WhenNoBookingRulesExist()
        {
            using var dbFactory = new InMemoryDbFactory();
            var service = new BookingRuleService(dbFactory.Context);

            var result = await service.UpdateBookingRulesAsync(Guid.NewGuid(), new UpdateBookingRulesDto());

            result.Status.Should().Be(ServiceResultStatus.NotFound);
        }

        [Fact]
        public async Task ShouldUpdateBasicFields_WhenValid()
        {
            using var dbFactory = new InMemoryDbFactory(ctx =>
                BookingRulesTestSeeder.SeedBookingRules(ctx));

            var dto = new UpdateBookingRulesDto
            {
                MaxAdvanceBookingDays = 15,
                BufferBetweenBookingsMinutes = 20,
                SlotDurationMinutes = 45,
                MinAdvanceBookingHours = 3
            };

            var service = new BookingRuleService(dbFactory.Context);
            var result = await service.UpdateBookingRulesAsync(BookingRulesTestSeeder.GetBookingRuleGuid(), dto);

            result.Status.Should().Be(ServiceResultStatus.Success);

            var updated = await dbFactory.Context.BookingRules.FirstAsync();
            updated.MaxAdvanceBookingDays.Should().Be(15);
            updated.BufferBetweenBookingsMinutes.Should().Be(20);
            updated.SlotDurationMinutes.Should().Be(45);
            updated.MinAdvanceBookingHours.Should().Be(3);
        }

        [Fact]
        public async Task ShouldUpdateOpeningHours_WhenProvided()
        {
            var openingHourId = Guid.NewGuid();

            using var dbFactory = new InMemoryDbFactory(ctx =>
                BookingRulesTestSeeder.SeedBookingRules(ctx, r =>
                {
                    r.OpeningHours.Add(new BookingRuleOpeningHour
                    {
                        Id = openingHourId,
                        BookingRulesId = r.Id,
                        DayOfWeek = DayOfWeek.Monday,
                        OpenTimeUtc = new TimeOnly(9, 0),
                        CloseTimeUtc = new TimeOnly(17, 0)
                    });
                }));

            var dto = new UpdateBookingRulesDto
            {
                OpeningHours = new List<UpdateBookingRuleOpeningHourDto>
                {
                    new UpdateBookingRuleOpeningHourDto
                    {
                        Id = openingHourId,
                        OpenTimeUtc = new TimeOnly(8, 0),
                        CloseTimeUtc = new TimeOnly(16, 0),
                        IsClosed = false
                    }
                }
            };

            var service = new BookingRuleService(dbFactory.Context);
            var result = await service.UpdateBookingRulesAsync(BookingRulesTestSeeder.GetBookingRuleGuid(), dto);

            result.Status.Should().Be(ServiceResultStatus.Success);

            var updatedHour = await dbFactory.Context.BookingRuleOpeningHours.FindAsync(openingHourId);
            updatedHour!.OpenTimeUtc.Should().Be(new TimeOnly(8, 0));
            updatedHour.CloseTimeUtc.Should().Be(new TimeOnly(16, 0));
            updatedHour.IsClosed.Should().BeFalse();
        }
    }
}
