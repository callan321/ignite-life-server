using FluentValidation;
using IgniteLifeApi.Api.Middleware;
using IgniteLifeApi.Api.OpenApi.Transformers;
using IgniteLifeApi.Application.Services.Implementations;
using IgniteLifeApi.Application.Services.Interfaces;
using IgniteLifeApi.Domain.Entities;
using IgniteLifeApi.Infrastructure.Configuration;
using IgniteLifeApi.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;
using System.Text;

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

// Identity (single admin user)
builder.Services.AddIdentityCore<AdminUser>(o =>
{
    o.Password.RequiredLength = 12;
    o.Password.RequireDigit = true;
    o.Password.RequireUppercase = true;
    o.Password.RequireLowercase = true;
    o.Password.RequireNonAlphanumeric = true;
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


// TODO CORS (set your SPA origin or disable while local)
builder.Services.AddCors(o => o.AddPolicy("spa", p =>
{
    // p.WithOrigins("https://your-frontend.example")
    //  .AllowAnyHeader().AllowAnyMethod().AllowCredentials();

    // Development fallback (no credentials with wildcard):
    p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
}));

var app = builder.Build();

// OpenAPI
app.MapOpenApi();

// Health
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready");

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseHttpsRedirection();

app.UseCors("spa");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }
