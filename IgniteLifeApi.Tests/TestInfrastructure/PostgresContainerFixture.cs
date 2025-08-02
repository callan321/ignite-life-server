using DotNet.Testcontainers.Builders;
using Microsoft.Extensions.Logging;
using Testcontainers.PostgreSql;

namespace IgniteLifeApi.Tests.TestInfrastructure
{
    public sealed class PostgresContainerFixture : IAsyncLifetime
    {
        static PostgresContainerFixture()
        {
            Environment.SetEnvironmentVariable("TESTCONTAINERS_RYUK_DISABLED", "true");
            if (OperatingSystem.IsLinux())
            {
                Environment.SetEnvironmentVariable("TESTCONTAINERS_HOST_OVERRIDE", "host.docker.internal");
            }
        }

        private static readonly ILoggerFactory LoggerFactory =
            Microsoft.Extensions.Logging.LoggerFactory.Create(builder =>
            {
                builder
                    .SetMinimumLevel(LogLevel.Error)
                    .AddFilter("Microsoft.EntityFrameworkCore", LogLevel.None) // Covers migrations + commands
                    .AddSimpleConsole();
            });

        private static string GetDockerEndpoint()
        {
            // On Windows, use named pipe; on Linux/Mac, use Unix socket
            if (OperatingSystem.IsWindows())
            {
                return "npipe://./pipe/docker_engine";
            }
            return "unix:///var/run/docker.sock";
        }

        private readonly PostgreSqlContainer _container;

        public PostgresContainerFixture()
        {
            _container = new PostgreSqlBuilder()
                .WithImage("postgres:17-alpine")
                .WithUsername("postgres")
                .WithPassword("postgres")
                .WithDatabase("testdb")
                .WithCleanUp(true)
                .WithDockerEndpoint(GetDockerEndpoint())
                .WithWaitStrategy(
                    Wait.ForUnixContainer().UntilPortIsAvailable(5432)
                )
                .WithLogger(LoggerFactory.CreateLogger<PostgresContainerFixture>())
                .Build();
        }

        public string ConnectionString => _container.GetConnectionString();

        public Task InitializeAsync() => _container.StartAsync();

        public Task DisposeAsync() => _container.DisposeAsync().AsTask();
    }
}
