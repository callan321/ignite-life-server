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
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace IgniteLifeApi.Tests.Infrastructure
{
    public class ApiPostgresTestApplicationFactory : WebApplicationFactory<Program>
    {
        private string? _uniqueDbName;
        private string? _baseConnectionString;

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
            _baseConnectionString = config.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Missing DefaultConnection.");

            // Step 2: Create a unique Postgres database
            var uniqueConnectionString = PostgresTestDatabaseHelper.CreateUniqueDatabase(_baseConnectionString, out _uniqueDbName);

            // Step 3: Override connection string in configuration
            builder.ConfigureAppConfiguration((_, configBuilder) =>
            {
                configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] = uniqueConnectionString
                });
            });

            // Step 4: Start the actual host
            var host = base.CreateHost(builder);

            // Step 5: Run migrations & seed
            using var db = PostgresTestDatabaseHelper.CreateMigratedContext<ApplicationDbContext>(uniqueConnectionString);
            BookingRulesTestSeeder.SeedBookingRules(db);

            return host;
        }

        public override async ValueTask DisposeAsync()
        {
            await base.DisposeAsync();

            if (_baseConnectionString != null && _uniqueDbName != null)
            {
                PostgresTestDatabaseHelper.DropDatabase(_baseConnectionString, _uniqueDbName);
            }
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
