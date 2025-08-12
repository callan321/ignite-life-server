using IgniteLifeApi.Application.Dtos.Auth;
using IgniteLifeApi.Application.Services.Interfaces;
using IgniteLifeApi.Domain.Entities;
using IgniteLifeApi.Domain.Interfaces;
using IgniteLifeApi.Infrastructure.Configuration;
using IgniteLifeApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using IgniteLifeApi.Api.Middleware;

namespace IgniteLifeApi.Application.Services.Implementations
{
    public class TokenService : ITokenService
    {
        private static readonly JwtSecurityTokenHandler JwtHandler = new();
        private readonly JwtSettings _jwt;
        private readonly ApplicationDbContext _db;
        private readonly IHttpContextAccessor _http;

        public TokenService(IOptions<JwtSettings> jwtOptions, ApplicationDbContext db, IHttpContextAccessor http)
        {
            _jwt = jwtOptions.Value;
            _db = db;
            _http = http;
        }

        public async Task<TokenResponse> GenerateTokensAsync(IJwtUser user, bool isPersistent)
        {
            var (accessToken, accessExp) = GenerateAccessToken(user);

            var refreshExp = isPersistent
                ? DateTime.UtcNow.AddDays(_jwt.RememberMeRefreshTokenExpiryDays)
                : DateTime.UtcNow.AddHours(_jwt.SlidingRefreshTokenExpiryHours);

            var rawRefresh = GenerateRefreshTokenRaw();
            var refreshHash = Hash(rawRefresh);

            _db.Add(new RefreshToken
            {
                UserId = user.Id,
                TokenHash = refreshHash,
                ExpiresAtUtc = refreshExp,
                IsPersistent = isPersistent,
                IpAddress = _http.HttpContext?.Connection.RemoteIpAddress?.ToString(),
                UserAgent = _http.HttpContext?.Request.Headers.UserAgent.ToString(),
                // CreatedAtUtc = DateTime.UtcNow, // if you track it
            });

            await _db.SaveChangesAsync();

            SetAccessCookie(accessToken, accessExp);
            SetRefreshCookie(rawRefresh, refreshExp);
            SetCsrfCookie(refreshExp); // align CSRF lifetime with refresh

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

            var user = await _db.AdminUsers.AsNoTracking().FirstOrDefaultAsync(u => u.Id == existing.UserId);
            if (user is null) return null;

            var newRaw = GenerateRefreshTokenRaw();
            var newHash = Hash(newRaw);
            var newExp = existing.IsPersistent
                ? DateTime.UtcNow.AddDays(_jwt.RememberMeRefreshTokenExpiryDays)
                : DateTime.UtcNow.AddHours(_jwt.SlidingRefreshTokenExpiryHours);

            existing.RevokedAtUtc = DateTime.UtcNow;
            existing.ReplacedByTokenHash = newHash;

            _db.RefreshTokens.Add(new RefreshToken
            {
                UserId = user.Id,
                TokenHash = newHash,
                ExpiresAtUtc = newExp,
                IsPersistent = existing.IsPersistent,
                IpAddress = _http.HttpContext?.Connection.RemoteIpAddress?.ToString(),
                UserAgent = _http.HttpContext?.Request.Headers.UserAgent.ToString()
            });

            var (jwt, jwtExp) = GenerateAccessToken(user);
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

        private (string token, DateTime expiresUtc) GenerateAccessToken(IJwtUser user)
        {
            var exp = DateTime.UtcNow.AddMinutes(_jwt.AccessTokenExpiryMinutes);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Email, user.Email),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // jti for traceability/blacklist
            };

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

            // Clear CSRF cookie too
            var xsrfExpired = CsrfDoubleSubmitMiddleware.BuildCookieOptions(DateTime.UtcNow.AddDays(-1), _jwt.CookieDomain,
                string.IsNullOrWhiteSpace(_jwt.CookiePath) ? "/" : _jwt.CookiePath);
            ctx.Response.Cookies.Append(CsrfDoubleSubmitMiddleware.CsrfCookieName, string.Empty, xsrfExpired);
        }
    }
}
