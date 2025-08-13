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

        // ------- New roles-only entrypoint (no IJwtUser) -------
        public async Task<TokenResponse> GenerateTokensAsync(Guid userId, bool isPersistent)
        {
            var appUser = await _users.FindByIdAsync(userId.ToString());
            if (appUser is null) throw new InvalidOperationException("User not found.");

            var isAdmin = await _users.IsInRoleAsync(appUser, "Admin");

            var (accessToken, accessExp) = GenerateAccessToken(appUser.Id, appUser.Email, isAdmin);

            var refreshExp = isPersistent
                ? DateTime.UtcNow.AddDays(_jwt.RememberMeRefreshTokenExpiryDays)
                : DateTime.UtcNow.AddHours(_jwt.SlidingRefreshTokenExpiryHours);

            var rawRefresh = GenerateRefreshTokenRaw();
            var refreshHash = Hash(rawRefresh);

            _db.Add(new RefreshToken
            {
                UserId = appUser.Id,
                TokenHash = refreshHash,
                ExpiresAtUtc = refreshExp,
                IsPersistent = isPersistent,
                IpAddress = _http.HttpContext?.Connection.RemoteIpAddress?.ToString(),
                UserAgent = _http.HttpContext?.Request.Headers.UserAgent.ToString(),
            });

            await _db.SaveChangesAsync();

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

        public async Task<bool> ValidateRefreshTokenAsync(string refreshToken)
        {
            var entity = await FindRefresh(refreshToken);
            return entity is { RevokedAtUtc: null } && entity.ExpiresAtUtc > DateTime.UtcNow;
        }

        public async Task RevokeRefreshTokenAsync(string refreshToken)
        {
            var entity = await FindRefresh(refreshToken);
            if (entity is null) return;

            entity.RevokedAtUtc = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            ClearAuthCookies();
        }

        public async Task<TokenResponse?> RefreshTokensAsync(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken)) return null;

            var existing = await FindRefresh(refreshToken);
            if (existing is null || existing.RevokedAtUtc is not null || existing.ExpiresAtUtc <= DateTime.UtcNow)
                return null;

            var appUser = await _users.FindByIdAsync(existing.UserId.ToString());
            if (appUser is null) return null;

            var isAdmin = await _users.IsInRoleAsync(appUser, "Admin");

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
                IsPersistent = existing.IsPersistent,
                IpAddress = _http.HttpContext?.Connection.RemoteIpAddress?.ToString(),
                UserAgent = _http.HttpContext?.Request.Headers.UserAgent.ToString()
            });

            var (jwt, jwtExp) = GenerateAccessToken(appUser.Id, appUser.Email, isAdmin);
            await _db.SaveChangesAsync();

            SetAccessCookie(jwt, jwtExp);
            SetRefreshCookie(newRaw, newExp);
            SetCsrfCookie(newExp);

            return new TokenResponse
            {
                AccessToken = new AccessTokenDto { Token = jwt, ExpiresAt = jwtExp },
                RefreshToken = new RefreshTokenDto { Token = newRaw, ExpiresAt = newExp }
            };
        }

        // ------- Token builder (pure, roles-first) -------
        private (string token, DateTime expiresUtc) GenerateAccessToken(Guid userId, string? email, bool isAdmin)
        {
            var exp = DateTime.UtcNow.AddMinutes(_jwt.AccessTokenExpiryMinutes);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userId.ToString()),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            if (!string.IsNullOrWhiteSpace(email))
                claims.Add(new Claim(ClaimTypes.Email, email!));

            if (isAdmin)
            {
                claims.Add(new Claim(ClaimTypes.Role, "Admin")); // for RequireRole("Admin")
                claims.Add(new Claim("isAdmin", "true"));        // optional convenience
            }

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

        private async Task<RefreshToken?> FindRefresh(string raw)
        {
            var hash = Hash(raw);
            return await _db.RefreshTokens.FirstOrDefaultAsync(r => r.TokenHash == hash);
        }

        private static string GenerateRefreshTokenRaw() =>
            Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        private static string Hash(string raw)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
            return Convert.ToHexString(bytes);
        }

        // ---------- Cookies ----------
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
                string.IsNullOrWhiteSpace(_jwt.CookiePath) ? "/" : _jwt.CookiePath);
            ctx.Response.Cookies.Append(CsrfDoubleSubmitMiddleware.CsrfCookieName, string.Empty, xsrfExpired);
        }
    }
}
