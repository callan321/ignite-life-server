using Xunit.Abstractions;

namespace IgniteLifeApi.Tests.TestInfrastructure
{
    public abstract class TestBase(ITestOutputHelper? output = null)
    {
        protected readonly ITestOutputHelper? Output = output;

        protected void Log(string message)
        {
            Output?.WriteLine($"[{DateTime.UtcNow:HH:mm:ss}] {message}");
        }
    }
}
