using IgniteLifeApi.Infrastructure.Configuration;
using Microsoft.Extensions.Configuration;

namespace IgniteLifeApi.Tests.Infrastructure
{
    public static class TestJwtConfigHelper
    {
        public static JwtSettings GetJwtSettingsFromTestConfig()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddJsonFile("appsettings.Testing.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var settings = config.GetSection("JwtSettings").Get<JwtSettings>();
            if (settings is null)
                throw new InvalidOperationException("Missing or invalid JwtSettings.");

            return settings;
        }
    }
}
