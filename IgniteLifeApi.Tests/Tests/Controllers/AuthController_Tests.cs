using FluentAssertions;
using IgniteLifeApi.Tests.Infrastructure;
using System.Net;

namespace IgniteLifeApi.Tests.Tests.Controllers
{
    public class AuthController_Tests
    {
        private const string BaseAuthRoute = "api/auth";

        [Theory]
        [InlineData("POST", "logout")]
        [InlineData("GET", "status")]
        public async Task ProtectedEndpoints_Anonymous_Returns401(string method, string action)
        {
            await using var factory = new ApiPostgresTestApplicationFactory();
            var client = factory.CreateAnonymousClient();

            var request = new HttpRequestMessage(new HttpMethod(method), $"{BaseAuthRoute}/{action}");
            var resp = await client.SendAsync(request);

            resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Theory]
        [InlineData("POST", "logout")]
        [InlineData("GET", "status")]
        public async Task VerifiedUserPolicy_MissingClaim_Returns403(string method, string action)
        {
            await using var factory = new ApiPostgresTestApplicationFactory();
            var client = factory.CreateAuthenticatedButUnverifiedClient();

            var request = new HttpRequestMessage(new HttpMethod(method), $"{BaseAuthRoute}/{action}");
            var resp = await client.SendAsync(request);

            resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }
    }
}
