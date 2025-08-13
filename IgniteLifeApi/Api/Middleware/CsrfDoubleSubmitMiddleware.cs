using Microsoft.Extensions.Primitives;
using System.Security.Cryptography;

namespace IgniteLifeApi.Api.Middleware
{
    /// <summary>
    /// Double-submit CSRF protection for cookie-based auth:
    /// - Server sets a non-HttpOnly cookie "XSRF-TOKEN" with a random value.
    /// - Client must echo that value in "X-CSRF-Token" header for unsafe methods.
    /// This runs after authentication; it only applies when auth cookies are present.
    /// </summary>
    public class CsrfDoubleSubmitMiddleware
    {
        private readonly RequestDelegate _next;
        private static readonly PathString[] _exemptPaths =
        {
            // Allow login to proceed without CSRF (no session yet).
            new PathString("/api/auth/login"),
            // Health and docs
            new PathString("/health"),
            new PathString("/health/ready")
        };

        public const string CsrfCookieName = "XSRF-TOKEN";
        public const string CsrfHeaderName = "X-CSRF-Token";

        public CsrfDoubleSubmitMiddleware(RequestDelegate next) => _next = next;

        public async Task Invoke(HttpContext context)
        {
            // Skip safe methods
            if (HttpMethods.IsGet(context.Request.Method) ||
                HttpMethods.IsHead(context.Request.Method) ||
                HttpMethods.IsOptions(context.Request.Method) ||
                HttpMethods.IsTrace(context.Request.Method))
            {
                await _next(context);
                return;
            }

            // Exempt some paths (e.g., login)
            var path = context.Request.Path;
            if (_exemptPaths.Any(p => path.StartsWithSegments(p, StringComparison.OrdinalIgnoreCase)))
            {
                await _next(context);
                return;
            }

            // Require token only if we detect auth cookies usage (access/refresh present).
            // If you also support Authorization header flows, you might choose to skip when no cookies exist.
            var hasAuthCookie = context.Request.Cookies.Keys.Any(k =>
                string.Equals(k, "AccessToken", StringComparison.OrdinalIgnoreCase) || // fallback if named that way
                string.Equals(k, "RefreshToken", StringComparison.OrdinalIgnoreCase) ||
                k.Contains("Access", StringComparison.OrdinalIgnoreCase) ||
                k.Contains("Refresh", StringComparison.OrdinalIgnoreCase));

            if (!hasAuthCookie)
            {
                await _next(context);
                return;
            }

            var cookie = context.Request.Cookies[CsrfCookieName];
            context.Request.Headers.TryGetValue(CsrfHeaderName, out var header);
            if (string.IsNullOrWhiteSpace(cookie) || StringValues.IsNullOrEmpty(header) || !CryptographicOperations.FixedTimeEquals(
                    System.Text.Encoding.UTF8.GetBytes(cookie),
                    System.Text.Encoding.UTF8.GetBytes(header.ToString())))
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync("CSRF token missing or invalid.");
                return;
            }

            await _next(context);
        }

        /// <summary>
        /// Helper to generate a new CSRF token value.
        /// </summary>
        public static string GenerateCsrfToken() =>
            Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

        /// <summary>
        /// Helper to build cookie options consistent with auth cookies.
        /// </summary>
        public static CookieOptions BuildCookieOptions(DateTime expiresUtc, string? domain, string? path)
        {
            var opts = new CookieOptions
            {
                HttpOnly = false,
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = string.IsNullOrWhiteSpace(path) ? "/" : path,
                Expires = new DateTimeOffset(expiresUtc)
            };
            if (!string.IsNullOrWhiteSpace(domain))
                opts.Domain = domain;
            return opts;
        }
    }
}
