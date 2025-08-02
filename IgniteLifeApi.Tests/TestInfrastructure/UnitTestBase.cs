using IgniteLifeApi.Data;
using IgniteLifeApi.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit.Abstractions;

namespace IgniteLifeApi.Tests.TestInfrastructure
{
    public abstract class UnitTestBase : TestBase, IDisposable
    {
        protected readonly ApplicationDbContext DbContext;
        protected readonly BookingRuleBlockedPeriodService BlockedPeriodService;

        protected UnitTestBase(ITestOutputHelper? output = null) : base(output)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            DbContext = new ApplicationDbContext(options);

            BlockedPeriodService = new BookingRuleBlockedPeriodService(
                DbContext,
                NullLogger<BookingRuleBlockedPeriodService>.Instance);
        }

        public void Dispose()
        {
            DbContext.Dispose();
        }
    }
}
