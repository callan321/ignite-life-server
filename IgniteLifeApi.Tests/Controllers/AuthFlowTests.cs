using IgniteLifeApi.Tests.TestInfrastructure;

namespace IgniteLifeApi.Tests.Controllers
{
    [Collection("IntegrationTests")]
    public class AuthFlowTests
    {
        private const string AdminEmail = "admin@example.com";
        private const string AdminPassword = "Admin!23456789";

        private readonly ApiPostgresTestApplicationFactory _factory;

        public AuthFlowTests(ApiPostgresTestApplicationFactory factory) => _factory = factory;
    }
}
