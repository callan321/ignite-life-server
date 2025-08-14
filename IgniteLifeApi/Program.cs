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
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// ===================================
// Add Controllers, Health Checks, OpenAPI
// ===================================
builder.Services.AddControllers();
builder.Services.AddHealthChecks();
builder.Services.AddOpenApi(options =>
{
    options.AddOperationTransformer<JsonOnlyOperationTransformer>();
    options.AddOperationTransformer<AuthCookieOperationTransformer>();
    options.AddDocumentTransformer<CookieAuthDocumentTransformer>();
});

// ===================================
// Bind configuration to strongly-typed settings
// ===================================
builder.Services.AddOptions<JwtSettings>()
    .Bind(builder.Configuration.GetSection("JwtSettings"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddOptions<IdentitySettings>()
    .Bind(builder.Configuration.GetSection("IdentitySettings"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddOptions<CorsSettings>()
    .Bind(builder.Configuration.GetSection("Cors"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

// ===================================
// Get settings instances
// ===================================
var jwt = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()!;
var identitySettings = builder.Configuration.GetSection("IdentitySettings").Get<IdentitySettings>()!;
var corsSettings = builder.Configuration.GetSection("Cors").Get<CorsSettings>()!;

// ===================================
// Database Context
// ===================================
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ===================================
// FluentValidation
// ===================================
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddFluentValidationAutoValidation();

// ===================================
// Identity Setup
// ===================================
builder.Services.AddIdentityCore<ApplicationUser>(o =>
{
    o.Password.RequiredLength = identitySettings.PasswordRequiredLength;
    o.Password.RequireDigit = identitySettings.RequireDigit;
    o.Password.RequireUppercase = identitySettings.RequireUppercase;
    o.Password.RequireLowercase = identitySettings.RequireLowercase;
    o.Password.RequireNonAlphanumeric = identitySettings.RequireNonAlphanumeric;

    o.Lockout.AllowedForNewUsers = identitySettings.LockoutAllowedForNewUsers;
    o.Lockout.MaxFailedAccessAttempts = identitySettings.LockoutMaxFailedAccessAttempts;
    o.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(identitySettings.LockoutDefaultLockoutMinutes);
})
.AddRoles<IdentityRole<Guid>>()
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddSignInManager<SignInManager<ApplicationUser>>();

builder.Services.AddHttpContextAccessor();

// ===================================
// Application Services
// ===================================
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<BookingRuleService>();

// ===================================
// Authentication
// ===================================
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Secret)),
            ValidateIssuerSigningKey = true,
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            RoleClaimType = ClaimTypes.Role
        };

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

// ===================================
// Authorization Policies
// ===================================
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("VerifiedUser", p =>
        p.RequireAuthenticatedUser()
        .RequireClaim("verified", "true"))
    .AddPolicy("AdminUser", p =>
        p.RequireAuthenticatedUser()
        .RequireClaim("verified", "true")
        .RequireRole("Admin"));

// ===================================
// CORS
// ===================================
builder.Services.AddCors(o => o.AddPolicy("spa", p =>
{
    if (!string.IsNullOrWhiteSpace(corsSettings.SpaOrigin))
    {
        p.WithOrigins(corsSettings.SpaOrigin)
         .AllowAnyHeader()
         .AllowAnyMethod()
         .AllowCredentials();
    }
    else
    {
        p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    }
}));

// ===================================
// Rate Limiting
// ===================================
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("auth", httpContext =>
    {
        var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: ip,
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 5,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                AutoReplenishment = true
            });
    });
});

// ===================================
// Forwarded Headers
// ===================================
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

// ===================================
// Build App
// ===================================
var app = builder.Build();

app.MapOpenApi();
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready");

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseMiddleware<CsrfDoubleSubmitMiddleware>();
app.UseHttpsRedirection();
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseForwardedHeaders();
app.UseCors("spa");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program { }
