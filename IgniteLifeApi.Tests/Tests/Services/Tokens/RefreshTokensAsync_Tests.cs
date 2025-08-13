using System.Security.Claims;
using FluentAssertions;
using IgniteLifeApi.Domain.Entities;
using IgniteLifeApi.Tests.Infrastructure;

namespace IgniteLifeApi.Tests.Tests.Services.Tokens
{
    public class RefreshTokensAsync_Tests
    {
        private const string ValidPassword = "StrongPass123!";

        [Fact]
        public async Task Should_ReturnNull_WhenTokenNullOrEmpty()
        {
            using var factory = new TokenServiceTestFactory();

            (await factory.TokenService.RefreshTokensAsync(null!)).Should().BeNull();
            (await factory.TokenService.RefreshTokensAsync(string.Empty)).Should().BeNull();
            (await factory.TokenService.RefreshTokensAsync("   ")).Should().BeNull();
        }

        [Fact]
        public async Task Should_ReturnNull_WhenTokenUnknown()
        {
            using var factory = new TokenServiceTestFactory();

            var resp = await factory.TokenService.RefreshTokensAsync("nope");
            resp.Should().BeNull();
        }

        [Fact]
        public async Task Should_RotateRefreshToken_AndReturnNewTokens()
        {
            using var factory = new TokenServiceTestFactory();
            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                Email = "rotate@example.com",
                UserName = "rotate@example.com",
                EmailConfirmed = true
            };
            await factory.UserManager.CreateAsync(user, ValidPassword);

            var initial = await factory.TokenService.GenerateTokensAsync(user.Id, isPersistent: false);

            var refreshed = await factory.TokenService.RefreshTokensAsync(initial.RefreshToken.Token);

            refreshed.Should().NotBeNull();
            refreshed!.AccessToken.Token.Should().NotBe(initial.AccessToken.Token);
            refreshed.RefreshToken.Token.Should().NotBe(initial.RefreshToken.Token);

            // Old token should be revoked, new one added
            var tokens = factory.DbContext.RefreshTokens.Where(r => r.UserId == user.Id).ToList();
            tokens.Should().HaveCount(2);

            var old = tokens.Single(t => t.RevokedAtUtc != null);
            var @new = tokens.Single(t => t.RevokedAtUtc == null);

            old.ReplacedByTokenHash.Should().NotBeNullOrEmpty();
            @new.IsPersistent.Should().BeFalse(); // preserve original persistence flag
            @new.ExpiresAtUtc.Should().BeAfter(DateTime.UtcNow);
        }

        [Fact]
        public async Task Should_ReturnNull_WhenTokenAlreadyRevoked()
        {
            using var factory = new TokenServiceTestFactory();
            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                Email = "revoked@example.com",
                UserName = "revoked@example.com",
                EmailConfirmed = true
            };
            await factory.UserManager.CreateAsync(user, ValidPassword);

            var initial = await factory.TokenService.GenerateTokensAsync(user.Id, false);

            await factory.TokenService.RevokeRefreshTokenAsync(initial.RefreshToken.Token);

            var again = await factory.TokenService.RefreshTokensAsync(initial.RefreshToken.Token);
            again.Should().BeNull();
        }

        [Fact]
        public async Task Should_ReturnNull_WhenTokenExpired()
        {
            using var factory = new TokenServiceTestFactory();
            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                Email = "old@example.com",
                UserName = "old@example.com",
                EmailConfirmed = true
            };
            await factory.UserManager.CreateAsync(user, ValidPassword);

            var initial = await factory.TokenService.GenerateTokensAsync(user.Id, false);

            var stored = factory.DbContext.RefreshTokens.Single(r => r.UserId == user.Id);
            stored.ExpiresAtUtc = DateTime.UtcNow.AddMinutes(-5);
            await factory.DbContext.SaveChangesAsync();

            var resp = await factory.TokenService.RefreshTokensAsync(initial.RefreshToken.Token);

            resp.Should().BeNull();
        }

        [Fact]
        public async Task Should_ReturnNull_WhenUserDeleted()
        {
            using var factory = new TokenServiceTestFactory();
            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                Email = "gone@example.com",
                UserName = "gone@example.com",
                EmailConfirmed = true
            };
            await factory.UserManager.CreateAsync(user, ValidPassword);

            var initial = await factory.TokenService.GenerateTokensAsync(user.Id, false);

            // Remove user after token was issued
            factory.DbContext.Users.Remove(user);
            await factory.DbContext.SaveChangesAsync();

            var resp = await factory.TokenService.RefreshTokensAsync(initial.RefreshToken.Token);

            resp.Should().BeNull();
        }

        [Fact]
        public async Task Should_IncludeAdminClaim_InNewAccessToken_WhenUserIsAdmin()
        {
            using var factory = new TokenServiceTestFactory();
            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                Email = "admin-refresh@example.com",
                UserName = "admin-refresh@example.com",
                EmailConfirmed = true
            };
            await factory.UserManager.CreateAsync(user, ValidPassword);
            await factory.RoleManager.CreateAsync(new Microsoft.AspNetCore.Identity.IdentityRole<Guid>
            {
                Id = Guid.NewGuid(),
                Name = "Admin",
                NormalizedName = "ADMIN"
            });
            await factory.UserManager.AddToRoleAsync(user, "Admin");

            var initial = await factory.TokenService.GenerateTokensAsync(user.Id, false);

            var refreshed = await factory.TokenService.RefreshTokensAsync(initial.RefreshToken.Token);
            refreshed.Should().NotBeNull();

            var jwt = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler()
                .ReadJwtToken(refreshed!.AccessToken.Token);

            jwt.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "Admin");
            jwt.Claims.Should().Contain(c => c.Type == "isAdmin" && c.Value == "true");
        }

        [Fact]
        public async Task Should_PreservePersistenceFlag_WhenRefreshing()
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

            // Issue a persistent (remember-me) refresh token
            var initial = await factory.TokenService.GenerateTokensAsync(user.Id, isPersistent: true);

            var refreshed = await factory.TokenService.RefreshTokensAsync(initial.RefreshToken.Token);
            refreshed.Should().NotBeNull();

            var tokens = factory.DbContext.RefreshTokens.Where(r => r.UserId == user.Id).ToList();
            tokens.Should().HaveCount(2);

            var newActive = tokens.Single(t => t.RevokedAtUtc == null);
            newActive.IsPersistent.Should().BeTrue();
        }
    }
}
