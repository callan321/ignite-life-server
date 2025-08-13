using IgniteLifeApi.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;

namespace IgniteLifeApi.Tests.TestInfrastructure
{
    public class ApiPostgresTestApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureTestServices(services =>
            {
                // Make our Test handler the default auth scheme for tests
                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = TestAuthHandler.Scheme;
                    options.DefaultChallengeScheme = TestAuthHandler.Scheme;
                    options.DefaultScheme = TestAuthHandler.Scheme;
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.Scheme, _ => { });

                // Policies used by your app/tests
                services.AddAuthorization(o =>
                {
                    o.AddPolicy("VerifiedUser", p => p.RequireClaim("verified", "true"));
                    o.AddPolicy("AdminUser", p => p.RequireClaim("isAdmin", "true"));
                });
            });
        }

        public HttpClient CreateAnonymousClient() => CreateClient();

        public HttpClient CreateVerifiedClient()
        {
            var c = CreateClient(new() { AllowAutoRedirect = false });
            c.DefaultRequestHeaders.Add("Authorization", "Test verified=true");
            return c;
        }

        public HttpClient CreateAdminClient()
        {
            var c = CreateClient(new() { AllowAutoRedirect = false });
            c.DefaultRequestHeaders.Add("Authorization", "Test verified=true isAdmin=true");
            return c;
        }

        public HttpClient CreateAuthenticatedButUnverifiedClient()
        {
            var c = CreateClient(new() { AllowAutoRedirect = false });
            // Authenticated with Test scheme but NO verified/isAdmin claims
            c.DefaultRequestHeaders.Add("Authorization", "Test");
            return c;
        }

        protected override IHost CreateHost(IHostBuilder builder)
        {
            var host = base.CreateHost(builder);

            using var scope = host.Services.CreateScope();
            var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

            var cs = config.GetConnectionString("DefaultConnection")
                     ?? throw new InvalidOperationException("Missing DefaultConnection.");
            var csb = new NpgsqlConnectionStringBuilder(cs);
            var dbName = csb.Database;
            if (string.IsNullOrWhiteSpace(dbName))
                throw new InvalidOperationException("Connection string has no Database.");

            NpgsqlConnection.ClearAllPools();

            csb.Database = "postgres";
            using var admin = new NpgsqlConnection(csb.ConnectionString);
            admin.Open();

            using (var takeLock = new NpgsqlCommand("SELECT pg_advisory_lock(hashtext(@k));", admin))
            {
                takeLock.Parameters.AddWithValue("k", $"ignite-tests:{dbName}");
                takeLock.ExecuteNonQuery();
            }

            try
            {
                using (var kill = new NpgsqlCommand(@"
                SELECT pg_terminate_backend(pid)
                FROM pg_stat_activity
                WHERE datname = @db AND pid <> pg_backend_pid();", admin))
                {
                    kill.Parameters.AddWithValue("db", dbName);
                    kill.ExecuteNonQuery();
                }

                using (var drop = new NpgsqlCommand($@"DROP DATABASE IF EXISTS ""{dbName}"";", admin))
                    drop.ExecuteNonQuery();

                using (var create = new NpgsqlCommand($@"CREATE DATABASE ""{dbName}"";", admin))
                    create.ExecuteNonQuery();
            }
            finally
            {
                using var release = new NpgsqlCommand("SELECT pg_advisory_unlock(hashtext(@k));", admin);
                release.Parameters.AddWithValue("k", $"ignite-tests:{dbName}");
                release.ExecuteNonQuery();
            }

            using var scope2 = host.Services.CreateScope();
            var db = scope2.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Database.Migrate();

            return host;
        }
    }
}
