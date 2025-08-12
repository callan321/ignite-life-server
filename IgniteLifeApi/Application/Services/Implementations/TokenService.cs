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

namespace IgniteLifeApi.Application.Services.Implementations
{
    /// <summary>
    /// Handles JWT access token creation, validation, refresh token rotation,
    /// and secure cookie management for the admin user authentication flow.
    /// </summary>
    public class TokenService : ITokenService
    {
        private readonly JwtSettings _jwt;
        private readonly ApplicationDbContext _dbContext;
        private readonly IHttpContextAccessor _http;

        public TokenService(IOptions<JwtSettings> jwtOptions, ApplicationDbContext dbContext, IHttpContextAccessor http)
        {
            _jwt = jwtOptions.Value;
            _dbContext = dbContext;
            _http = http;
        }

        // ==============================
        //  PUBLIC API
        // ==============================

        /// <summary>
        /// Generates a new access token + refresh token pair for the given user.
        /// Stores the refresh token hash in the DB and sets both tokens as
        /// HttpOnly cookies on the current response.
        /// </summary>
        public async Task<TokenResponse> GenerateTokensAsync(IJwtUser user, bool isPersistent)
        {
            // Create short-lived access token
            var (accessToken, accessExp) = GenerateAccessToken(user);

            // Determine refresh token expiry based on "remember me"
            var refreshExp = isPersistent
                ? DateTime.UtcNow.AddDays(_jwt.RememberMeRefreshTokenExpiryDays)
                : DateTime.UtcNow.AddHours(_jwt.SlidingRefreshTokenExpiryHours);

            // Generate secure random refresh token
            var rawRefresh = GenerateRefreshTokenRaw();
            var refreshHash = Hash(rawRefresh);

            // Persist refresh token (hashed) to DB
            _dbContext.Add(new RefreshToken
            {
                UserId = user.Id,
                TokenHash = refreshHash,
                ExpiresAtUtc = refreshExp,
                IsPersistent = isPersistent,
                IpAddress = _http.HttpContext?.Connection.RemoteIpAddress?.ToString(),
                UserAgent = _http.HttpContext?.Request.Headers.UserAgent.ToString()
            });

            await _dbContext.SaveChangesAsync();

            // Send tokens to client via secure cookies
            SetAccessCookie(accessToken, accessExp);
            SetRefreshCookie(rawRefresh, refreshExp);

            return new TokenResponse
            {
                AccessToken = new AccessTokenDto { Token = accessToken, ExpiresAt = accessExp },
                RefreshToken = new RefreshTokenDto { Token = rawRefresh, ExpiresAt = refreshExp }
            };
        }

        /// <summary>
        /// Validates an access token and returns the claims principal if valid.
        /// </summary>
        public ClaimsPrincipal? ValidateAccessToken(string token) =>
            ValidateToken(token, validateLifetime: true);

        /// <summary>
        /// Checks if a refresh token is still valid (exists, not revoked, not expired).
        /// </summary>
        public async Task<bool> ValidateRefreshTokenAsync(string refreshToken)
        {
            var entity = await FindRefresh(refreshToken);
            return entity is { RevokedAtUtc: null } && entity.ExpiresAtUtc > DateTime.UtcNow;
        }

        /// <summary>
        /// Marks the given refresh token as revoked and clears cookies.
        /// </summary>
        public async Task RevokeRefreshTokenAsync(string refreshToken)
        {
            var entity = await FindRefresh(refreshToken);
            if (entity is null) return;

            entity.RevokedAtUtc = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();

            ClearAuthCookies();
        }

        /// <summary>
        /// Validates and rotates an existing refresh token, issuing a new token pair.
        /// Replaces the old refresh token in the DB and sets new cookies.
        /// </summary>
        public async Task<TokenResponse?> RefreshTokensAsync(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken)) return null;

            // Validate old refresh token
            var existing = await FindRefresh(refreshToken);
            if (existing is null || existing.RevokedAtUtc is not null || existing.ExpiresAtUtc <= DateTime.UtcNow)
                return null;

            // Get the user for this refresh token
            var user = await _dbContext.AdminUsers
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == existing.UserId);
            if (user is null) return null;

            // Generate new refresh token
            var newRaw = GenerateRefreshTokenRaw();
            var newHash = Hash(newRaw);
            var newExp = existing.IsPersistent
                ? DateTime.UtcNow.AddDays(_jwt.RememberMeRefreshTokenExpiryDays)
                : DateTime.UtcNow.AddHours(_jwt.SlidingRefreshTokenExpiryHours);

            // Revoke old refresh token and link to new
            existing.RevokedAtUtc = DateTime.UtcNow;
            existing.ReplacedByTokenHash = newHash;

            // Store new refresh token
            _dbContext.RefreshTokens.Add(
                new RefreshToken
                {
                    UserId = user.Id,
                    TokenHash = newHash,
                    ExpiresAtUtc = newExp,
                    IsPersistent = existing.IsPersistent,
                    IpAddress = _http.HttpContext?.Connection.RemoteIpAddress?.ToString(),
                    UserAgent = _http.HttpContext?.Request.Headers.UserAgent.ToString()
                });

            // Generate new access token
            var (jwt, jwtExp) = GenerateAccessToken(user);
            await _dbContext.SaveChangesAsync();

            // Set cookies for new tokens
            SetAccessCookie(jwt, jwtExp);
            SetRefreshCookie(newRaw, newExp);

            return new TokenResponse
            {
                AccessToken = new AccessTokenDto { Token = jwt, ExpiresAt = jwtExp },
                RefreshToken = new RefreshTokenDto { Token = newRaw, ExpiresAt = newExp }
            };
        }

        // ==============================
        //  INTERNAL HELPERS
        // ==============================

        /// <summary>
        /// Creates a signed JWT access token for the specified user.
        /// </summary>
        private (string token, DateTime expiresUtc) GenerateAccessToken(IJwtUser user)
        {
            var exp = DateTime.UtcNow.AddMinutes(_jwt.AccessTokenExpiryMinutes);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Email, user.Email),
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

            return (new JwtSecurityTokenHandler().WriteToken(jwt), exp);
        }

        /// <summary>
        /// Validates a JWT token against configured parameters.
        /// </summary>
        private ClaimsPrincipal? ValidateToken(string token, bool validateLifetime)
        {
            var handler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwt.Secret);

            try
            {
                return handler.ValidateToken(token, new TokenValidationParameters
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

        /// <summary>
        /// Finds a refresh token entity by its raw token string (after hashing).
        /// </summary>
        private async Task<RefreshToken?> FindRefresh(string raw)
        {
            var hash = Hash(raw);

            return await _dbContext.RefreshTokens.FirstOrDefaultAsync(r => r.TokenHash == hash);
        }

        /// <summary>
        /// Creates a secure, random refresh token string.
        /// </summary>
        private static string GenerateRefreshTokenRaw() =>
            Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        /// <summary>
        /// Returns a SHA-256 hex string hash of the given token.
        /// </summary>
        private static string Hash(string raw)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
            return Convert.ToHexString(bytes);
        }

        // ==============================
        //  COOKIE HELPERS
        // ==============================

        /// <summary>
        /// Sets the access token as an HttpOnly, Secure cookie.
        /// </summary>
        private void SetAccessCookie(string jwt, DateTime expiresUtc)
        {
            if (_http.HttpContext is null) return;

            _http.HttpContext.Response.Cookies.Append(_jwt.AccessTokenCookieName, jwt, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = _jwt.CookiePath,
                Domain = _jwt.CookieDomain,
                Expires = new DateTimeOffset(expiresUtc)
            });
        }

        /// <summary>
        /// Sets the refresh token as an HttpOnly, Secure cookie.
        /// </summary>
        private void SetRefreshCookie(string raw, DateTime expiresUtc)
        {
            if (_http.HttpContext is null) return;

            _http.HttpContext.Response.Cookies.Append(_jwt.RefreshTokenCookieName, raw, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = _jwt.CookiePath,
                Domain = _jwt.CookieDomain,
                Expires = new DateTimeOffset(expiresUtc)
            });
        }

        /// <summary>
        /// Clears both access and refresh token cookies by setting them expired.
        /// </summary>
        private void ClearAuthCookies()
        {
            if (_http.HttpContext is null) return;

            var expired = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = _jwt.CookiePath,
                Domain = _jwt.CookieDomain,
                Expires = DateTimeOffset.UtcNow.AddDays(-1)
            };

            _http.HttpContext.Response.Cookies.Append(_jwt.AccessTokenCookieName, string.Empty, expired);
            _http.HttpContext.Response.Cookies.Append(_jwt.RefreshTokenCookieName, string.Empty, expired);
        }
    }
}
