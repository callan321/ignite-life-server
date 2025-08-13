using IgniteLifeApi.Infrastructure.Configuration;
using Microsoft.Extensions.Configuration;

namespace IgniteLifeApi.Tests.Infrastructure
{
    public static class TestIdentityConfigHelper
    {
        public static IdentitySettings GetIdentitySettingsFromTestConfig()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddJsonFile("appsettings.Testing.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var settings = config.GetSection("IdentitySettings").Get<IdentitySettings>();
            if (settings is null)
                throw new InvalidOperationException("Missing or invalid IdentitySettings.");

            return settings;
        }
    }
}
