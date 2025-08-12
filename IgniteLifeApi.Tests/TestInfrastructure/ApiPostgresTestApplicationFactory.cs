using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;

namespace IgniteLifeApi.Tests.TestInfrastructure
{
    public class ApiPostgresTestApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
        }

        protected override IHost CreateHost(IHostBuilder builder)
        {
            var host = base.CreateHost(builder);
            TestDataSeeder.SeedAsync(host.Services).GetAwaiter().GetResult();
            return host;
        }
    }
}
