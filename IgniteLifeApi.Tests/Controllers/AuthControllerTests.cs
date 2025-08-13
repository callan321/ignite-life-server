using FluentAssertions;
using IgniteLifeApi.Api.Controllers;
using IgniteLifeApi.Tests.TestInfrastructure;
using IgniteLifeApi.Tests.Utilities;
using System.Net;

namespace IgniteLifeApi.Tests.Controllers
{
    [Collection("IntegrationTests")]
    public class AuthControllerTests : IClassFixture<ApiPostgresTestApplicationFactory>
    {
        private readonly ApiPostgresTestApplicationFactory _factory;
        private readonly string _baseAuthRoute;

        public AuthControllerTests(ApiPostgresTestApplicationFactory factory)
        {
            _factory = factory;
            _baseAuthRoute = ApiRoutes.ForController<AuthController>();
        }

        [Theory]
        [InlineData("POST", "logout")]
        [InlineData("GET", "status")]
        public async Task ProtectedEndpoints_Anonymous_Returns401(string method, string action)
        {
            var client = _factory.CreateAnonymousClient();
            var request = new HttpRequestMessage(new HttpMethod(method), $"{_baseAuthRoute}/{action}");

            var resp = await client.SendAsync(request);

            resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Theory]
        [InlineData("POST", "logout")]
        [InlineData("GET", "status")]
        public async Task VerifiedUserPolicy_MissingClaim_Returns403(string method, string action)
        {
            var client = _factory.CreateAuthenticatedButUnverifiedClient();
            var request = new HttpRequestMessage(new HttpMethod(method), $"{_baseAuthRoute}/{action}");

            var resp = await client.SendAsync(request);

            resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }


        [Fact]
        public async Task Logout_VerifiedButNoCookie_Returns204()
        {
            var client = _factory.CreateVerifiedClient();

            var resp = await client.PostAsync(ApiRoutes.ForController<AuthController>("logout"), content: null);

            resp.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task Logout_WithCookie_AllowsAndNot401Or403()
        {
            var client = _factory.CreateVerifiedClient();
            client.DefaultRequestHeaders.Add("Cookie", "refreshToken=dummy");

            var resp = await client.PostAsync(ApiRoutes.ForController<AuthController>("logout"), content: null);

            resp.StatusCode.Should()
                .NotBe(HttpStatusCode.Unauthorized)
                .And.NotBe(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task Refresh_NoCookie_Returns204()
        {
            var client = _factory.CreateAnonymousClient();

            var resp = await client.PostAsync(ApiRoutes.ForController<AuthController>("refresh"), content: null);

            resp.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task Refresh_WithCookie_AllowsAndNot401Or403()
        {
            var client = _factory.CreateAnonymousClient();
            client.DefaultRequestHeaders.Add("Cookie", "refreshToken=dummy");

            var resp = await client.PostAsync(ApiRoutes.ForController<AuthController>("refresh"), content: null);

            resp.StatusCode.Should()
                .NotBe(HttpStatusCode.Unauthorized)
                .And.NotBe(HttpStatusCode.Forbidden);
        }
    }
}