using IgniteLifeApi.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IgniteLifeApi.Tests
{
    public class DatabaseConnectionTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public DatabaseConnectionTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Should_Initialize_TestDB()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var canConnect = await db.Database.CanConnectAsync();

            Assert.True(canConnect, "Database should be created and reachable during tests.");
        }

        [Fact]
        public async Task Should_Delete_DB_Explicitly()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            await db.Database.EnsureDeletedAsync();
            var canConnect = await db.Database.CanConnectAsync();

            Assert.False(canConnect, "Database should be deleted and not reachable after deletion.");
        }
    }

}
