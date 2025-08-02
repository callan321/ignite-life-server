using IgniteLifeApi.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Xunit.Abstractions;

namespace IgniteLifeApi.Tests.TestInfrastructure
{
    public abstract class IntegrationTestBase
        : TestBase, IClassFixture<PostgresContainerFixture>, IAsyncLifetime
    {
        private readonly PostgresContainerFixture _fixture;
        protected readonly HttpClient Client;
        protected readonly ApplicationDbContext DbContext;

        public IntegrationTestBase(PostgresContainerFixture fixture, ITestOutputHelper? output = null)
            : base(output)
        {
            _fixture = fixture;

            var factory = new ApiTestApplicationFactory(_fixture.ConnectionString);
            Client = factory.CreateClient();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseNpgsql(_fixture.ConnectionString)
                .Options;

            DbContext = new ApplicationDbContext(options);
        }

        public virtual Task InitializeAsync() => Task.CompletedTask;

        public virtual Task DisposeAsync()
        {
            DbContext.Dispose();
            Client.Dispose();
            return Task.CompletedTask;
        }

        protected async Task<NpgsqlConnection> GetOpenConnectionAsync()
        {
            var conn = new NpgsqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            return conn;
        }
    }
}
