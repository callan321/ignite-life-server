using IgniteLifeApi.Api.Middleware;
using IgniteLifeApi.Application.Dtos.Auth;
using IgniteLifeApi.Application.Services.Interfaces;
using IgniteLifeApi.Domain.Entities;
using IgniteLifeApi.Infrastructure.Configuration;
using IgniteLifeApi.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace IgniteLifeApi.Application.Services.Implementations
{
    public class TokenService : ITokenService
    {
        private static readonly JwtSecurityTokenHandler JwtHandler = new();
        private readonly JwtSettings _jwt;
        private readonly ApplicationDbContext _db;
        private readonly IHttpContextAccessor _http;
        private readonly UserManager<ApplicationUser> _users;

        public TokenService(
            IOptions<JwtSettings> jwtOptions,
            ApplicationDbContext db,
            IHttpContextAccessor http,
            UserManager<ApplicationUser> users)
        {
            _jwt = jwtOptions.Value;
            _db = db;
            _http = http;
            _users = users;
        }

        /// <summary>Generates new access + refresh tokens for a given user, including role/verified claims.</summary>
        public async Task<TokenResponse> GenerateTokensAsync(Guid userId, bool isPersistent, CancellationToken cancellationToken = default)
        {
            var appUser = await _users.FindByIdAsync(userId.ToString());
            if (appUser is null)
                throw new InvalidOperationException("User not found.");

            if (!appUser.EmailConfirmed)
                throw new InvalidOperationException("User email not confirmed.");

            if (appUser.LockoutEnabled && appUser.LockoutEnd.GetValueOrDefault().UtcDateTime > DateTime.UtcNow)
                throw new InvalidOperationException("User account is locked.");

            var (accessToken, accessExp) = await GenerateAccessTokenAsync(appUser);

            var refreshExp = isPersistent
                ? DateTime.UtcNow.AddDays(_jwt.RememberMeRefreshTokenExpiryDays)
                : DateTime.UtcNow.AddHours(_jwt.SlidingRefreshTokenExpiryHours);

            var rawRefresh = GenerateRefreshTokenRaw();
            var refreshHash = Hash(rawRefresh);

            _db.RefreshTokens.Add(new RefreshToken
            {
                UserId = appUser.Id,
                TokenHash = refreshHash,
                ExpiresAtUtc = refreshExp,
                IsPersistent = isPersistent
            });

            await _db.SaveChangesAsync(cancellationToken);

            SetAccessCookie(accessToken, accessExp);
            SetRefreshCookie(rawRefresh, refreshExp);
            SetCsrfCookie(refreshExp);

            return new TokenResponse
            {
                AccessToken = new AccessTokenDto { Token = accessToken, ExpiresAt = accessExp },
                RefreshToken = new RefreshTokenDto { Token = rawRefresh, ExpiresAt = refreshExp }
            };
        }

        public ClaimsPrincipal? ValidateAccessToken(string token) =>
            ValidateToken(token, validateLifetime: true);

        public async Task<bool> ValidateRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            var entity = await FindRefresh(refreshToken, cancellationToken);
            return entity is { RevokedAtUtc: null } && entity.ExpiresAtUtc > DateTime.UtcNow;
        }

        public async Task RevokeRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            var entity = await FindRefresh(refreshToken, cancellationToken);
            if (entity is not null && entity.RevokedAtUtc is null)
            {
                entity.RevokedAtUtc = DateTime.UtcNow;
                await _db.SaveChangesAsync(cancellationToken);
            }

            ClearAuthCookies();
        }

        public async Task<TokenResponse?> RefreshTokensAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(refreshToken)) return null;

            var existing = await FindRefresh(refreshToken, cancellationToken);
            if (existing is null || existing.RevokedAtUtc is not null || existing.ExpiresAtUtc <= DateTime.UtcNow)
                return null;

            var appUser = await _users.FindByIdAsync(existing.UserId.ToString());
            if (appUser is null) return null;

            // Rotate refresh token
            var newRaw = GenerateRefreshTokenRaw();
            var newHash = Hash(newRaw);
            var newExp = existing.IsPersistent
                ? DateTime.UtcNow.AddDays(_jwt.RememberMeRefreshTokenExpiryDays)
                : DateTime.UtcNow.AddHours(_jwt.SlidingRefreshTokenExpiryHours);

            existing.RevokedAtUtc = DateTime.UtcNow;
            existing.ReplacedByTokenHash = newHash;

            _db.RefreshTokens.Add(new RefreshToken
            {
                UserId = appUser.Id,
                TokenHash = newHash,
                ExpiresAtUtc = newExp,
                IsPersistent = existing.IsPersistent
            });

            var (jwt, jwtExp) = await GenerateAccessTokenAsync(appUser);
            await _db.SaveChangesAsync(cancellationToken);

            SetAccessCookie(jwt, jwtExp);
            SetRefreshCookie(newRaw, newExp);
            SetCsrfCookie(newExp);

            return new TokenResponse
            {
                AccessToken = new AccessTokenDto { Token = jwt, ExpiresAt = jwtExp },
                RefreshToken = new RefreshTokenDto { Token = newRaw, ExpiresAt = newExp }
            };
        }

        // =========================
        // Claims & Access Token
        // =========================
        private async Task<List<Claim>> BuildUserClaimsAsync(ApplicationUser user)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            if (!string.IsNullOrWhiteSpace(user.Email))
                claims.Add(new Claim(ClaimTypes.Email, user.Email));

            // Mark verified (adjust if you have a different flag)
            var isVerified = user.EmailConfirmed;
            claims.Add(new Claim("verified", isVerified ? "true" : "false"));

            // Roles
            var roles = await _users.GetRolesAsync(user);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
                if (string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
                    claims.Add(new Claim("isAdmin", "true"));
            }

            // Security stamp for server-side invalidation
            if (!string.IsNullOrEmpty(user.SecurityStamp))
                claims.Add(new Claim("sstamp", user.SecurityStamp));

            return claims;
        }

        private async Task<(string token, DateTime expiresUtc)> GenerateAccessTokenAsync(ApplicationUser user)
        {
            var exp = DateTime.UtcNow.AddMinutes(_jwt.AccessTokenExpiryMinutes);
            var claims = await BuildUserClaimsAsync(user);

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var jwt = new JwtSecurityToken(
                issuer: _jwt.Issuer,
                audience: _jwt.Audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: exp,
                signingCredentials: creds
            );

            return (JwtHandler.WriteToken(jwt), exp);
        }

        private ClaimsPrincipal? ValidateToken(string token, bool validateLifetime)
        {
            var key = Encoding.UTF8.GetBytes(_jwt.Secret);
            try
            {
                return JwtHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _jwt.Issuer,
                    ValidateAudience = true,
                    ValidAudience = _jwt.Audience,
                    ValidateLifetime = validateLifetime,
                    ClockSkew = TimeSpan.Zero
                }, out _);
            }
            catch
            {
                return null;
            }
        }

        private async Task<RefreshToken?> FindRefresh(string raw, CancellationToken cancellationToken = default)
        {
            var hash = Hash(raw);
            return await _db.RefreshTokens.FirstOrDefaultAsync(r => r.TokenHash == hash, cancellationToken);
        }

        private static string GenerateRefreshTokenRaw() =>
            Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        private static string Hash(string raw)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
            return Convert.ToHexString(bytes);
        }

        // =========================
        // Cookies
        // =========================
        private CookieOptions BuildCookieOptions(DateTime expiresUtc)
        {
            var opts = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = string.IsNullOrWhiteSpace(_jwt.CookiePath) ? "/" : _jwt.CookiePath,
                Expires = new DateTimeOffset(expiresUtc)
            };

            if (!string.IsNullOrWhiteSpace(_jwt.CookieDomain))
                opts.Domain = _jwt.CookieDomain;

            return opts;
        }

        private void SetAccessCookie(string jwt, DateTime expiresUtc)
        {
            var ctx = _http.HttpContext;
            if (ctx is null) return;
            ctx.Response.Cookies.Append(_jwt.AccessTokenCookieName, jwt, BuildCookieOptions(expiresUtc));
        }

        private void SetRefreshCookie(string raw, DateTime expiresUtc)
        {
            var ctx = _http.HttpContext;
            if (ctx is null) return;
            ctx.Response.Cookies.Append(_jwt.RefreshTokenCookieName, raw, BuildCookieOptions(expiresUtc));
        }

        private void SetCsrfCookie(DateTime expiresUtc)
        {
            var ctx = _http.HttpContext;
            if (ctx is null) return;

            var token = CsrfDoubleSubmitMiddleware.GenerateCsrfToken();
            var opts = CsrfDoubleSubmitMiddleware.BuildCookieOptions(
                expiresUtc,
                _jwt.CookieDomain,
                string.IsNullOrWhiteSpace(_jwt.CookiePath) ? "/" : _jwt.CookiePath
            );
            ctx.Response.Cookies.Append(CsrfDoubleSubmitMiddleware.CsrfCookieName, token, opts);
        }

        private void ClearAuthCookies()
        {
            var ctx = _http.HttpContext;
            if (ctx is null) return;

            var expired = BuildCookieOptions(DateTime.UtcNow.AddDays(-1));
            ctx.Response.Cookies.Append(_jwt.AccessTokenCookieName, string.Empty, expired);
            ctx.Response.Cookies.Append(_jwt.RefreshTokenCookieName, string.Empty, expired);

            var xsrfExpired = CsrfDoubleSubmitMiddleware.BuildCookieOptions(
                DateTime.UtcNow.AddDays(-1),
                _jwt.CookieDomain,
                string.IsNullOrWhiteSpace(_jwt.CookiePath) ? "/" : _jwt.CookiePath
            );
            ctx.Response.Cookies.Append(CsrfDoubleSubmitMiddleware.CsrfCookieName, string.Empty, xsrfExpired);
        }
    }
}
