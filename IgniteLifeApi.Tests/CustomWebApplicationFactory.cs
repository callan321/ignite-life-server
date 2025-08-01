using IgniteLifeApi.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace IgniteLifeApi.Tests
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        public async Task InitializeAsync()
        {
            using var scope = Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await db.Database.MigrateAsync();
        }

        public new async Task DisposeAsync()
        {
            using var scope = Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await db.Database.EnsureDeletedAsync();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddSimpleConsole(options =>
                {
                    options.TimestampFormat = "[HH:mm:ss] ";
                    options.IncludeScopes = true;
                    options.SingleLine = true;
                });

                logging.SetMinimumLevel(LogLevel.Information);
                logging.AddFilter("Microsoft", LogLevel.Warning);
                logging.AddFilter("System", LogLevel.Warning);
                logging.AddFilter("IgniteLifeApi", LogLevel.Debug);
            });

            builder.ConfigureServices(services =>
            {
                var descriptor = services.FirstOrDefault(d =>
                    d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                var baseConnectionString = Environment.GetEnvironmentVariable("TEST_CONNECTION_STRING")
                    ?? throw new InvalidOperationException("TEST_CONNECTION_STRING environment variable is not set");

                // Generate unique DB name for test isolation
                var dbName = $"IgniteLifeTestDB_{Guid.NewGuid()}";
                var connectionString = $"{baseConnectionString};Database={dbName}";

                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseNpgsql(connectionString);
                    options.EnableSensitiveDataLogging();
                    options.EnableDetailedErrors();
                });
            });
        }
    }
}