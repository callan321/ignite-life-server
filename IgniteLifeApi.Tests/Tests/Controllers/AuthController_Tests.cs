using FluentAssertions;
using IgniteLifeApi.Tests.Infrastructure;
using System.Net;

namespace IgniteLifeApi.Tests.Tests.Controllers
{
    public class AuthController_Tests
    {
        private const string BaseAuthRoute = "api/auth";
        private const string LogoutRoute = "api/auth/logout";
        private const string RefreshRoute = "api/auth/refresh";

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

        [Fact]
        public async Task Logout_VerifiedButNoCookie_Returns204()
        {
            await using var factory = new ApiPostgresTestApplicationFactory();
            var client = factory.CreateVerifiedClient();

            var resp = await client.PostAsync(LogoutRoute, content: null);

            resp.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task Logout_WithCookie_AllowsAndNot401Or403()
        {
            await using var factory = new ApiPostgresTestApplicationFactory();
            var client = factory.CreateVerifiedClient();
            client.DefaultRequestHeaders.Add("Cookie", "refreshToken=dummy");

            var resp = await client.PostAsync(LogoutRoute, content: null);

            resp.StatusCode.Should()
                .NotBe(HttpStatusCode.Unauthorized)
                .And.NotBe(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task Refresh_NoCookie_Returns204()
        {
            await using var factory = new ApiPostgresTestApplicationFactory();
            var client = factory.CreateAnonymousClient();

            var resp = await client.PostAsync(RefreshRoute, content: null);

            resp.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task Refresh_WithCookie_AllowsAndNot401Or403()
        {
            await using var factory = new ApiPostgresTestApplicationFactory();
            var client = factory.CreateAnonymousClient();
            client.DefaultRequestHeaders.Add("Cookie", "refreshToken=dummy");

            var resp = await client.PostAsync(RefreshRoute, content: null);

            resp.StatusCode.Should()
                .NotBe(HttpStatusCode.Unauthorized)
                .And.NotBe(HttpStatusCode.Forbidden);
        }
    }
}
