namespace IgniteLifeApi.Tests.TestInfrastructure
{
    [CollectionDefinition("IntegrationTests")]
    public class IntegrationTestCollection : ICollectionFixture<ApiPostgresTestApplicationFactory>
    {
        // This class has no code, it’s just a marker for xUnit
    }
}
