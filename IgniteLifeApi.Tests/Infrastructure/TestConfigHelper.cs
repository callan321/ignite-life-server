using Microsoft.Extensions.Configuration;

namespace IgniteLifeApi.Tests.Infrastructure
{
    public static class TestConfigHelper
    {
        public static string GetBaseConnectionStringFromTestingConfig()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddJsonFile("appsettings.Testing.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            return config.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Missing DefaultConnection.");
        }
    }
}
