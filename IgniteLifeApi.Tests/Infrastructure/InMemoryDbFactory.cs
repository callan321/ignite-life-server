using IgniteLifeApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace IgniteLifeApi.Tests.Infrastructure
{
    /// <summary>
    /// Creates an in-memory ApplicationDbContext for service-level tests.
    /// Seeding is only performed if explicitly provided via customSeed.
    /// </summary>
    public sealed class InMemoryDbFactory : IDisposable
    {
        private readonly ServiceProvider _serviceProvider;
        public ApplicationDbContext Context { get; }

        public InMemoryDbFactory(Action<ApplicationDbContext>? customSeed = null)
        {
            // Create a fresh service provider for isolation
            var services = new ServiceCollection();

            // Unique DB name so each test run is isolated
            var dbName = $"TestDb_{Guid.NewGuid()}";

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase(dbName));

            _serviceProvider = services.BuildServiceProvider();

            Context = _serviceProvider.GetRequiredService<ApplicationDbContext>();

            // Only seed if explicitly requested
            customSeed?.Invoke(Context);
        }

        public void Dispose()
        {
            Context.Dispose();
            _serviceProvider.Dispose();
        }
    }
}
