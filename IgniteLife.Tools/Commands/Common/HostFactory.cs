using IgniteLifeApi.Domain.Entities;
using IgniteLifeApi.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace IgniteLife.Tools.Commands.Common
{
    public static class HostFactory
    {
        public static IHost Create(string envName, bool includeIdentity)
        {
            var builder = Host.CreateApplicationBuilder();

            // Pull config from the API project (base + env)
            var apiDir = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, "..", "IgniteLifeApi"));
            builder.Configuration
                .AddJsonFile(Path.Combine(apiDir, "appsettings.json"), optional: true, reloadOnChange: false)
                .AddJsonFile(Path.Combine(apiDir, $"appsettings.{envName}.json"), optional: true, reloadOnChange: false)
                .AddEnvironmentVariables();

            // DbContext
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                var conn = builder.Configuration.GetConnectionString("DefaultConnection")
                    ?? builder.Configuration["ConnectionStrings__DefaultConnection"]
                    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
                options.UseNpgsql(conn);
            });

            // Identity if needed
            if (includeIdentity)
            {
                builder.Services
                    .AddIdentityCore<ApplicationUser>()
                    .AddRoles<IdentityRole<Guid>>()
                    .AddEntityFrameworkStores<ApplicationDbContext>();
            }

            return builder.Build();
        }
    }
}
