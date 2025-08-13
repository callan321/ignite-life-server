using FluentAssertions;
using IgniteLifeApi.Domain.Entities;
using IgniteLifeApi.Tests.Infrastructure;

namespace IgniteLifeApi.Tests.Tests.Services.Tokens
{
    public class RevokeRefreshTokenAsync_Tests
    {
        private const string ValidPassword = "StrongPass123!";

        [Fact]
        public async Task Should_SetRevokedAt_WhenTokenExists()
        {
            using var factory = new TokenServiceTestFactory();
            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                Email = "revoke1@example.com",
                UserName = "revoke1@example.com",
                EmailConfirmed = true
            };
            await factory.UserManager.CreateAsync(user, ValidPassword);


            var tokens = await factory.TokenService.GenerateTokensAsync(user.Id, false);

            await factory.TokenService.RevokeRefreshTokenAsync(tokens.RefreshToken.Token);

            var stored = factory.DbContext.RefreshTokens.Single(r => r.UserId == user.Id);
            stored.RevokedAtUtc.Should().NotBeNull();
        }

        [Fact]
        public async Task Should_NoOp_WhenTokenUnknown()
        {
            using var factory = new TokenServiceTestFactory();

            // Just ensure it doesn't throw
            await factory.TokenService.RevokeRefreshTokenAsync("does-not-exist");

            // And obviously nothing in DB
            factory.DbContext.RefreshTokens.Should().BeEmpty();
        }

        [Fact]
        public async Task Should_BeIdempotent_WhenCalledTwice()
        {
            using var factory = new TokenServiceTestFactory();
            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                Email = "idemp@example.com",
                UserName = "idemp@example.com",
                EmailConfirmed = true
            };
            await factory.UserManager.CreateAsync(user, ValidPassword);

            var tokens = await factory.TokenService.GenerateTokensAsync(user.Id, false);

            await factory.TokenService.RevokeRefreshTokenAsync(tokens.RefreshToken.Token);
            var first = factory.DbContext.RefreshTokens.Single(r => r.UserId == user.Id);
            var firstRevokedAt = first.RevokedAtUtc;

            await factory.TokenService.RevokeRefreshTokenAsync(tokens.RefreshToken.Token);
            var second = factory.DbContext.RefreshTokens.Single(r => r.UserId == user.Id);

            second.RevokedAtUtc.Should().Be(firstRevokedAt); // unchanged
        }
    }
}
