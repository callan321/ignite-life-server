using FluentAssertions;
using IgniteLifeApi.Domain.Entities;
using IgniteLifeApi.Tests.Infrastructure;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Xunit;

namespace IgniteLifeApi.Tests.Tests.Services.Tokens
{
    public class ValidateAccessToken_Tests
    {
        private const string ValidPassword = "StrongPass123!";

        [Fact]
        public async Task Should_ReturnPrincipal_WhenTokenIsValid()
        {
            using var factory = new TokenServiceTestFactory();
            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                Email = "valid@example.com",
                UserName = "valid@example.com",
                EmailConfirmed = true
            };
            await factory.UserManager.CreateAsync(user, ValidPassword);

            // Generate valid token
            var tokens = await factory.TokenService.GenerateTokensAsync(user.Id, false);

            // Act
            var principal = factory.TokenService.ValidateAccessToken(tokens.AccessToken.Token);

            // Assert
            principal.Should().NotBeNull();
            principal!.FindFirst(ClaimTypes.NameIdentifier)!.Value
                .Should().Be(user.Id.ToString());
            principal.FindFirst(ClaimTypes.Email)!.Value
                .Should().Be(user.Email);
        }

        [Fact]
        public void Should_ReturnNull_WhenTokenIsInvalid()
        {
            using var factory = new TokenServiceTestFactory();

            var invalidToken = "this.is.not.a.jwt";

            var principal = factory.TokenService.ValidateAccessToken(invalidToken);

            principal.Should().BeNull();
        }



        [Fact]
        public void Should_ReturnNull_WhenTokenIsExpired()
        {
            using var factory = new TokenServiceTestFactory();

            // Build an already-expired JWT using the same settings as TokenService
            var jwtSettings = TestJwtConfigHelper.GetJwtSettingsFromTestConfig();

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var now = DateTime.UtcNow;

            var expiredJwt = new JwtSecurityToken(
                issuer: jwtSettings.Issuer,
                audience: jwtSettings.Audience,
                claims: [new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())],
                notBefore: now.AddMinutes(-10),
                expires: now.AddSeconds(-1),
                signingCredentials: creds
            );

            var expiredToken = new JwtSecurityTokenHandler().WriteToken(expiredJwt);

            var principal = factory.TokenService.ValidateAccessToken(expiredToken);

            principal.Should().BeNull();
        }
    }
}
