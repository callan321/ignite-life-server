using FluentAssertions;
using IgniteLifeApi.Domain.Entities;
using IgniteLifeApi.Infrastructure.Data;
using IgniteLifeApi.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;

public class BookingRulesConfigurationTests : IAsyncLifetime
{
    private string? _uniqueDbName;
    private string? _baseConnectionString;
    private ApplicationDbContext _db = default!;

    public async Task InitializeAsync()
    {
        _baseConnectionString = TestConfigHelper.GetBaseConnectionStringFromTestingConfig();
        var uniqueConnectionString = PostgresTestDatabaseHelper.CreateUniqueDatabase(_baseConnectionString, out _uniqueDbName);
        _db = PostgresTestDatabaseHelper.CreateMigratedContext<ApplicationDbContext>(uniqueConnectionString);
        await Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await _db.DisposeAsync();

        if (_baseConnectionString != null && _uniqueDbName != null)
        {
            PostgresTestDatabaseHelper.DropDatabase(_baseConnectionString, _uniqueDbName);
        }
    }

    [Fact]
    public async Task Should_EnforceSingletonConstraint()
    {
        _db.BookingRules.Add(new BookingRules
        {
            Id = Guid.NewGuid(),
            TimeZoneId = "UTC",
            MaxAdvanceBookingDays = 30,
            BufferBetweenBookingsMinutes = 15,
            SlotDurationMinutes = 30,
            MinAdvanceBookingHours = 1
        });

        await _db.SaveChangesAsync();

        _db.BookingRules.Add(new BookingRules
        {
            Id = Guid.NewGuid(),
            TimeZoneId = "UTC",
            MaxAdvanceBookingDays = 60,
            BufferBetweenBookingsMinutes = 10,
            SlotDurationMinutes = 45,
            MinAdvanceBookingHours = 2
        });

        Func<Task> act = async () => await _db.SaveChangesAsync();
        await act.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task Should_EnforceUniqueOpeningHoursPerDay()
    {
        var rulesId = Guid.NewGuid();

        _db.BookingRules.Add(new BookingRules
        {
            Id = rulesId,
            TimeZoneId = "UTC",
            MaxAdvanceBookingDays = 30,
            BufferBetweenBookingsMinutes = 15,
            SlotDurationMinutes = 30,
            MinAdvanceBookingHours = 1
        });

        await _db.SaveChangesAsync();

        // First Monday entry
        _db.BookingRuleOpeningHours.Add(new BookingRuleOpeningHour
        {
            Id = Guid.NewGuid(),
            BookingRulesId = rulesId,
            DayOfWeek = DayOfWeek.Monday,
            OpenTimeUtc = new TimeOnly(8, 0),
            CloseTimeUtc = new TimeOnly(17, 0),
            IsClosed = false
        });

        await _db.SaveChangesAsync();

        // Duplicate Monday entry (should fail)
        _db.BookingRuleOpeningHours.Add(new BookingRuleOpeningHour
        {
            Id = Guid.NewGuid(),
            BookingRulesId = rulesId,
            DayOfWeek = DayOfWeek.Monday,
            OpenTimeUtc = new TimeOnly(9, 0),
            CloseTimeUtc = new TimeOnly(18, 0),
            IsClosed = false
        });

        Func<Task> act = async () => await _db.SaveChangesAsync();
        await act.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task Should_CascadeDeleteOpeningHoursAndBlockedPeriods()
    {
        var rulesId = Guid.NewGuid();

        var rules = new BookingRules
        {
            Id = rulesId,
            TimeZoneId = "UTC",
            MaxAdvanceBookingDays = 30,
            BufferBetweenBookingsMinutes = 15,
            SlotDurationMinutes = 30,
            MinAdvanceBookingHours = 1,
            OpeningHours =
            {
                new BookingRuleOpeningHour
                {
                    Id = Guid.NewGuid(),
                    DayOfWeek = DayOfWeek.Monday,
                    OpenTimeUtc = new TimeOnly(8, 0),
                    CloseTimeUtc = new TimeOnly(17, 0),
                    IsClosed = false
                }
            },
            BlockedPeriods =
            {
                new BookingRuleBlockedPeriod
                {
                    Id = Guid.NewGuid(),
                    BookingRulesId = rulesId,
                    StartDateTimeUtc = DateTime.UtcNow,
                    EndDateTimeUtc = DateTime.UtcNow.AddHours(1),
                    Description = "Test Block"
                }
            }
        };

        _db.BookingRules.Add(rules);
        await _db.SaveChangesAsync();

        _db.BookingRules.Remove(rules);
        await _db.SaveChangesAsync();

        (await _db.BookingRuleOpeningHours.Where(h => h.BookingRulesId == rulesId).CountAsync()).Should().Be(0);
        (await _db.BookingRuleBlockedPeriods.Where(p => p.BookingRulesId == rulesId).CountAsync()).Should().Be(0);
    }
}
