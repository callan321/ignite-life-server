using FluentAssertions;
using IgniteLifeApi.Domain.Entities;
using IgniteLifeApi.Tests.Infrastructure;
using System.Security.Claims;

namespace IgniteLifeApi.Tests.Tests.Services.Tokens
{
    public class GenerateTokensAsync_Tests
    {
        private const string ValidPassword = "ValidPass123!@#"; // Matches IdentitySettings in appsettings.json

        [Fact]
        public async Task Should_GenerateTokens_ForExistingUser_NonPersistent()
        {
            using var factory = new TokenServiceTestFactory();
            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                Email = "user1@example.com",
                UserName = "user1@example.com",
                EmailConfirmed = true
            };
            await factory.UserManager.CreateAsync(user, ValidPassword);

            var tokens = await factory.TokenService.GenerateTokensAsync(user.Id, false);

            tokens.AccessToken.Should().NotBeNull();
            tokens.RefreshToken.Should().NotBeNull();
            tokens.AccessToken.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
        }

        [Fact]
        public async Task Should_GenerateTokens_ForExistingUser_Persistent()
        {
            using var factory = new TokenServiceTestFactory();
            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                Email = "user2@example.com",
                UserName = "user2@example.com",
                EmailConfirmed = true
            };
            await factory.UserManager.CreateAsync(user, ValidPassword);

            var tokens = await factory.TokenService.GenerateTokensAsync(user.Id, true);

            tokens.AccessToken.Should().NotBeNull();
            tokens.RefreshToken.Should().NotBeNull();
            tokens.RefreshToken.ExpiresAt.Should().BeAfter(DateTime.UtcNow.AddDays(1));
        }

        [Fact]
        public async Task Persistent_Should_LastLonger_Than_NonPersistent()
        {
            using var factory = new TokenServiceTestFactory();
            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                Email = "compare@example.com",
                UserName = "compare@example.com",
                EmailConfirmed = true
            };
            await factory.UserManager.CreateAsync(user, ValidPassword);

            var nonPersistent = await factory.TokenService.GenerateTokensAsync(user.Id, false);
            var persistent = await factory.TokenService.GenerateTokensAsync(user.Id, true);

            persistent.RefreshToken.ExpiresAt.Should().BeAfter(nonPersistent.RefreshToken.ExpiresAt);
        }

        [Fact]
        public async Task Tokens_ShouldBe_Unique_Per_Call()
        {
            using var factory = new TokenServiceTestFactory();
            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                Email = "unique@example.com",
                UserName = "unique@example.com",
                EmailConfirmed = true
            };
            await factory.UserManager.CreateAsync(user, ValidPassword);

            var tokens1 = await factory.TokenService.GenerateTokensAsync(user.Id, false);
            var tokens2 = await factory.TokenService.GenerateTokensAsync(user.Id, false);

            tokens1.AccessToken.Token.Should().NotBe(tokens2.AccessToken.Token);
            tokens1.RefreshToken.Token.Should().NotBe(tokens2.RefreshToken.Token);
        }

        [Fact]
        public async Task Should_IncludeAdminClaim_WhenUserIsAdmin()
        {
            using var factory = new TokenServiceTestFactory();
            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                Email = "admin@example.com",
                UserName = "admin@example.com",
                EmailConfirmed = true
            };
            await factory.UserManager.CreateAsync(user, ValidPassword);
            await factory.UserManager.AddToRoleAsync(user, "Admin");

            var tokens = await factory.TokenService.GenerateTokensAsync(user.Id, false);

            var jwt = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler()
                .ReadJwtToken(tokens.AccessToken.Token);

            jwt.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "Admin");
        }

        [Fact]
        public async Task Should_SaveRefreshToken_InDatabase()
        {
            using var factory = new TokenServiceTestFactory();
            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                Email = "persist@example.com",
                UserName = "persist@example.com",
                EmailConfirmed = true
            };
            await factory.UserManager.CreateAsync(user, ValidPassword);

            var tokens = await factory.TokenService.GenerateTokensAsync(user.Id, false);

            factory.DbContext.RefreshTokens.Should()
                .ContainSingle(r => r.UserId == user.Id && r.IsPersistent == false);
        }

        // --- Failure Scenarios ---

        [Fact]
        public async Task Should_Throw_IfUserNotFound()
        {
            using var factory = new TokenServiceTestFactory();
            var nonExistentUserId = Guid.NewGuid();

            await FluentActions.Invoking(() =>
                factory.TokenService.GenerateTokensAsync(nonExistentUserId, false))
                .Should()
                .ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task Should_Not_GenerateTokens_IfEmailNotConfirmed()
        {
            using var factory = new TokenServiceTestFactory();
            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                Email = "unconfirmed@example.com",
                UserName = "unconfirmed@example.com",
                EmailConfirmed = false
            };
            await factory.UserManager.CreateAsync(user, ValidPassword);

            await FluentActions.Invoking(() =>
                factory.TokenService.GenerateTokensAsync(user.Id, false))
                .Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("*email not confirmed*");
        }

        [Fact]
        public async Task Should_Not_GenerateTokens_IfUserLockedOut()
        {
            using var factory = new TokenServiceTestFactory();
            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                Email = "locked@example.com",
                UserName = "locked@example.com",
                EmailConfirmed = true,
                LockoutEnabled = true,
                LockoutEnd = DateTimeOffset.UtcNow.AddHours(1)
            };
            await factory.UserManager.CreateAsync(user, ValidPassword);

            await FluentActions.Invoking(() =>
                factory.TokenService.GenerateTokensAsync(user.Id, false))
                .Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("*locked*");
        }
    }
}
