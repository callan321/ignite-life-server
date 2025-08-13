using FluentAssertions;
using IgniteLifeApi.Domain.Entities;
using IgniteLifeApi.Tests.Infrastructure;

namespace IgniteLifeApi.Tests.Tests.Services.Tokens
{
    public class ValidateRefreshTokenAsync_Tests
    {
        private const string ValidPassword = "StrongPass123!";

        [Fact]
        public async Task Should_ReturnTrue_ForActiveRefreshToken()
        {
            using var factory = new TokenServiceTestFactory();
            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                Email = "ok@example.com",
                UserName = "ok@example.com",
                EmailConfirmed = true
            };
            await factory.UserManager.CreateAsync(user, ValidPassword);

            var tokens = await factory.TokenService.GenerateTokensAsync(user.Id, isPersistent: false);

            var valid = await factory.TokenService.ValidateRefreshTokenAsync(tokens.RefreshToken.Token);

            valid.Should().BeTrue();
        }

        [Fact]
        public async Task Should_ReturnFalse_ForUnknownToken()
        {
            using var factory = new TokenServiceTestFactory();

            var valid = await factory.TokenService.ValidateRefreshTokenAsync("not-a-real-refresh");

            valid.Should().BeFalse();
        }

        [Fact]
        public async Task Should_ReturnFalse_WhenTokenRevoked()
        {
            using var factory = new TokenServiceTestFactory();
            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                Email = "revoke@example.com",
                UserName = "revoke@example.com",
                EmailConfirmed = true
            };
            await factory.UserManager.CreateAsync(user, ValidPassword);

            var tokens = await factory.TokenService.GenerateTokensAsync(user.Id, false);

            await factory.TokenService.RevokeRefreshTokenAsync(tokens.RefreshToken.Token);

            var valid = await factory.TokenService.ValidateRefreshTokenAsync(tokens.RefreshToken.Token);

            valid.Should().BeFalse();
        }

        [Fact]
        public async Task Should_ReturnFalse_WhenTokenExpired()
        {
            using var factory = new TokenServiceTestFactory();
            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                Email = "expired@example.com",
                UserName = "expired@example.com",
                EmailConfirmed = true
            };
            await factory.UserManager.CreateAsync(user, ValidPassword);

            var tokens = await factory.TokenService.GenerateTokensAsync(user.Id, false);

            // Manually set the single stored refresh token to the past
            var rt = factory.DbContext.RefreshTokens.Single(r => r.UserId == user.Id);
            rt.ExpiresAtUtc = DateTime.UtcNow.AddMinutes(-1);
            await factory.DbContext.SaveChangesAsync();

            var valid = await factory.TokenService.ValidateRefreshTokenAsync(tokens.RefreshToken.Token);

            valid.Should().BeFalse();
        }
    }
}
