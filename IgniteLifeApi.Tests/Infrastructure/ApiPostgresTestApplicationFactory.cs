using IgniteLifeApi.Infrastructure.Data;
using IgniteLifeApi.Tests.Seeders;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;

namespace IgniteLifeApi.Tests.Infrastructure
{
    public class ApiPostgresTestApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureTestServices(services =>
            {
                ConfigureTestAuthentication(services);
                ConfigureTestAuthorization(services);
            });
        }

        public HttpClient CreateAnonymousClient() => CreateClient();

        public HttpClient CreateVerifiedClient() =>
            CreateClientWithAuthHeader("Test verified=true");

        public HttpClient CreateAdminClient() =>
            CreateClientWithAuthHeader("Test verified=true isAdmin=true");

        public HttpClient CreateAuthenticatedButUnverifiedClient() =>
            CreateClientWithAuthHeader("Test");

        protected override IHost CreateHost(IHostBuilder builder)
        {
            // Step 1: Build a temporary host to get the original connection string
            var tempHost = builder.Build();
            var config = tempHost.Services.GetRequiredService<IConfiguration>();
            var originalConnectionString = config.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Missing DefaultConnection.");

            // Step 2: Create a unique database name for isolation
            var uniqueDbName = $"ignite_test_{Guid.NewGuid():N}";

            // Step 3: Build a new connection string pointing to the unique DB
            var csb = new NpgsqlConnectionStringBuilder(originalConnectionString)
            {
                Database = uniqueDbName
            };

            // Step 4: Override connection string in configuration for the real host
            builder.ConfigureAppConfiguration((_, configBuilder) =>
            {
                configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] = csb.ToString()
                });
            });

            // Step 5: Start the actual host
            var host = base.CreateHost(builder);

            // Step 6: Create, migrate, and seed the new DB
            EnsureDatabaseCreated(csb.ToString());
            InitializeDatabase(host);

            return host;
        }

        private void EnsureDatabaseCreated(string connectionString)
        {
            // Create the DB if it doesn't exist by connecting to "postgres"
            var csb = new NpgsqlConnectionStringBuilder(connectionString) { Database = "postgres" };
            using var adminConn = new NpgsqlConnection(csb.ConnectionString);
            adminConn.Open();

            var dbName = new NpgsqlConnectionStringBuilder(connectionString).Database;
            using var cmd = new NpgsqlCommand($@"CREATE DATABASE ""{dbName}"";", adminConn);
            cmd.ExecuteNonQuery();
        }

        private void InitializeDatabase(IHost host)
        {
            using var scope = host.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Database.Migrate();
            BookingRulesTestSeeder.SeedBookingRules(db);
        }

        private void ConfigureTestAuthentication(IServiceCollection services)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = TestAuthHandler.Scheme;
                options.DefaultChallengeScheme = TestAuthHandler.Scheme;
                options.DefaultScheme = TestAuthHandler.Scheme;
            })
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.Scheme, _ => { });
        }

        private void ConfigureTestAuthorization(IServiceCollection services)
        {
            services.AddAuthorization(o =>
            {
                o.AddPolicy("VerifiedUser", p => p.RequireClaim("verified", "true"));
                o.AddPolicy("AdminUser", p => p.RequireClaim("isAdmin", "true"));
            });
        }

        private HttpClient CreateClientWithAuthHeader(string headerValue)
        {
            var client = CreateClient(new() { AllowAutoRedirect = false });
            client.DefaultRequestHeaders.Add("Authorization", headerValue);
            return client;
        }
    }
}
