using FluentValidation;
using IgniteLifeApi.Api.Middleware;
using IgniteLifeApi.Api.OpenApi.Transformers;
using IgniteLifeApi.Application.Services.Implementations;
using IgniteLifeApi.Application.Services.Interfaces;
using IgniteLifeApi.Domain.Entities;
using IgniteLifeApi.Infrastructure.Configuration;
using IgniteLifeApi.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;
using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Controllers, health, OpenAPI
builder.Services.AddControllers();
builder.Services.AddHealthChecks();
builder.Services.AddOpenApi(options =>
{
    options.AddOperationTransformer<JsonOnlyOperationTransformer>();
    options.AddOperationTransformer<AuthCookieOperationTransformer>();
    options.AddDocumentTransformer<CookieAuthDocumentTransformer>();
});

// DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddFluentValidationAutoValidation();

// Options
builder.Services.AddOptions<JwtSettings>()
    .Bind(builder.Configuration.GetSection("JwtSettings"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

var spaOrigin = builder.Configuration["Cors:SpaOrigin"]; // e.g. https://app.example

// Identity (single admin user)
builder.Services.AddIdentityCore<AdminUser>(o =>
{
    o.Password.RequiredLength = 12;
    o.Password.RequireDigit = true;
    o.Password.RequireUppercase = true;
    o.Password.RequireLowercase = true;
    o.Password.RequireNonAlphanumeric = true;

    // Lockout so CheckPasswordSignInAsync(..., lockoutOnFailure:true) actually locks
    o.Lockout.AllowedForNewUsers = true;
    o.Lockout.MaxFailedAccessAttempts = 5;
    o.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
})
.AddRoles<IdentityRole<Guid>>()
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddSignInManager<SignInManager<AdminUser>>();

builder.Services.AddHttpContextAccessor();

// App services
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<BookingRuleBlockedPeriodService>(); // if used by controllers

// AuthN / AuthZ
var jwt = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()!;

// Authentication
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = true; // prod
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Secret)),
            ValidateIssuerSigningKey = true,
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        // Read JWT from HttpOnly cookie
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                var token = ctx.Request.Cookies[jwt.AccessTokenCookieName];
                if (!string.IsNullOrEmpty(token))
                    ctx.Token = token;
                return Task.CompletedTask;
            }
        };
    });

// Authorization policies
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("VerifiedUser", p => p.RequireAuthenticatedUser());

// CORS — credentials for SPA
builder.Services.AddCors(o => o.AddPolicy("spa", p =>
{
    if (!string.IsNullOrWhiteSpace(spaOrigin))
    {
        p.WithOrigins(spaOrigin)
         .AllowAnyHeader()
         .AllowAnyMethod()
         .AllowCredentials();
    }
    else
    {
        // Dev fallback (no credentials across wildcard; useful for tools like Swagger UI),
        // but your cookie-auth SPA **won't** work cross-site with this.
        p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    }
}));

// Rate limiting
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("auth", httpContext =>
    {
        // partition key = caller IP (after forwarded-headers)
        var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: ip,
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,                   // 5 requests
                Window = TimeSpan.FromMinutes(1),  // per minute
                QueueLimit = 5,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                AutoReplenishment = true
            });
    });
});

// CSRF (double-submit): register middleware dependencies (no DI needed)

// Forwarded headers (read from proxies)
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    // Consider adding KnownProxies/KnownNetworks in locked-down environments.
});

var app = builder.Build();

// OpenAPI
app.MapOpenApi();

// Health
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready");

// Global error handling
app.UseMiddleware<GlobalExceptionMiddleware>();

// HTTPS & HSTS
app.UseHttpsRedirection();
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

// Forwarded headers MUST run before rate limiter so client IP is correct
app.UseForwardedHeaders();

// CORS early (before auth)
app.UseCors("spa");

// Rate limiter
app.UseRateLimiter();

// Authentication/Authorization
app.UseAuthentication();
app.UseAuthorization();

// CSRF protection for unsafe methods that rely on cookie auth
app.UseMiddleware<CsrfDoubleSubmitMiddleware>();

app.MapControllers();

app.Run();

public partial class Program { }
