using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public sealed class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public new const string Scheme = "Test";
    private static readonly char[] separator = [' ', ';'];

    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder) { }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var header = Request.Headers.Authorization.ToString();
        if (string.IsNullOrWhiteSpace(header) ||
            !header.StartsWith(Scheme, StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var args = header[Scheme.Length..].Trim();
        var kv = ParseArgs(args);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "TestUser") // always authenticated
        };

        if (kv.TryGetValue("verified", out var verified) &&
            verified.Equals("true", StringComparison.OrdinalIgnoreCase))
        {
            claims.Add(new Claim("verified", "true"));
        }

        if (kv.TryGetValue("isAdmin", out var isAdmin) &&
            isAdmin.Equals("true", StringComparison.OrdinalIgnoreCase))
        {
            claims.Add(new Claim("isAdmin", "true"));
        }

        var identity = new ClaimsIdentity(claims, Scheme);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    }

    protected override Task HandleForbiddenAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    }

    private static Dictionary<string, string> ParseArgs(string s)
    {
        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(s)) return dict;

        foreach (var part in s.Split(separator, StringSplitOptions.RemoveEmptyEntries))
        {
            var eq = part.IndexOf('=');
            if (eq <= 0 || eq == part.Length - 1) continue;
            dict[part[..eq].Trim()] = part[(eq + 1)..].Trim();
        }
        return dict;
    }
}
