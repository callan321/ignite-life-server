using IgniteLifeApi.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace IgniteLifeApi.Tests.TestInfrastructure
{
    public class ApiTestApplicationFactory(string connectionString) : WebApplicationFactory<Program>
    {
        private readonly string _connectionString = connectionString;

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureAppConfiguration((context, config) =>
            {
                var dict = new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] = _connectionString
                };
                config.AddInMemoryCollection(dict);
            });

            builder.ConfigureServices(services =>
            {
                // Run migrations during test startup
                using var scope = services.BuildServiceProvider().CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Database.Migrate();
            });

            builder.ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddSimpleConsole();
                logging.SetMinimumLevel(LogLevel.Warning);

                // Keep your app logs
                logging.AddFilter("IgniteLifeApi", LogLevel.Information);

                // Show only SQL errors
                logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Error);

                // Hide migration info
                logging.AddFilter("Microsoft.EntityFrameworkCore.Migrations", LogLevel.None);
            });
        }
    }
}
