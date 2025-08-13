using IgniteLifeApi.Application.Services.Implementations;
using IgniteLifeApi.Application.Services.Interfaces;
using IgniteLifeApi.Domain.Entities;
using IgniteLifeApi.Infrastructure.Configuration; // << needed for JwtSettings
using IgniteLifeApi.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace IgniteLifeApi.Tests.Infrastructure
{
    /// <summary>
    /// Fully-wired TokenService test factory with in-memory EF Core and Identity + Roles.
    /// Supports optional JWT overrides for testing (expiry, issuer, audience, etc.).
    /// </summary>
    public sealed class TokenServiceTestFactory : IDisposable
    {
        private readonly ServiceProvider _serviceProvider;

        public ApplicationDbContext DbContext { get; }
        public ITokenService TokenService { get; }
        public UserManager<ApplicationUser> UserManager { get; }
        public RoleManager<IdentityRole<Guid>> RoleManager { get; }

        /// <param name="customSeed">Optional DB seed for users/data.</param>
        /// <param name="configureJwt">Optional override for JwtSettings used by TokenService.</param>
        public TokenServiceTestFactory(
            Action<ApplicationDbContext>? customSeed = null,
            Action<JwtSettings>? configureJwt = null)
        {
            var services = new ServiceCollection();

            // Unique in-memory DB per factory instance
            var dbName = $"TokenTestDb_{Guid.NewGuid()}";
            services.AddDbContext<ApplicationDbContext>(opt =>
                opt.UseInMemoryDatabase(dbName));

            // Identity + roles
            services.AddIdentityCore<ApplicationUser>(options =>
            {
                var identitySettings = TestIdentityConfigHelper.GetIdentitySettingsFromTestConfig();
                options.Password.RequiredLength = identitySettings.PasswordRequiredLength;
                options.Password.RequireDigit = identitySettings.RequireDigit;
                options.Password.RequireUppercase = identitySettings.RequireUppercase;
                options.Password.RequireLowercase = identitySettings.RequireLowercase;
                options.Password.RequireNonAlphanumeric = identitySettings.RequireNonAlphanumeric;
                options.Lockout.AllowedForNewUsers = identitySettings.LockoutAllowedForNewUsers;
                options.Lockout.MaxFailedAccessAttempts = identitySettings.LockoutMaxFailedAccessAttempts;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(identitySettings.LockoutDefaultLockoutMinutes);
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddRoleManager<RoleManager<IdentityRole<Guid>>>()
            .AddSignInManager();

            // TokenService needs HttpContext for cookies
            services.AddHttpContextAccessor();

            // Load JwtSettings from test config, then apply optional overrides
            var jwtSettings = TestJwtConfigHelper.GetJwtSettingsFromTestConfig();
            configureJwt?.Invoke(jwtSettings);
            services.AddSingleton(Options.Create(jwtSettings));

            // TokenService
            services.AddScoped<ITokenService, TokenService>();

            _serviceProvider = services.BuildServiceProvider();

            DbContext = _serviceProvider.GetRequiredService<ApplicationDbContext>();
            UserManager = _serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            RoleManager = _serviceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
            TokenService = _serviceProvider.GetRequiredService<ITokenService>();

            // Ensure default roles exist via RoleManager (not direct DbContext)
            SeedDefaultRoles(RoleManager);

            // Optional caller-provided seeding (users, etc.)
            customSeed?.Invoke(DbContext);
        }

        private static void SeedDefaultRoles(RoleManager<IdentityRole<Guid>> roleManager)
        {
            if (!roleManager.RoleExistsAsync("Admin").GetAwaiter().GetResult())
            {
                roleManager.CreateAsync(new IdentityRole<Guid>
                {
                    Id = Guid.NewGuid(),
                    Name = "Admin",
                    NormalizedName = "ADMIN"
                }).GetAwaiter().GetResult();
            }
        }

        public void Dispose()
        {
            DbContext.Dispose();
            _serviceProvider.Dispose();
        }
    }
}
