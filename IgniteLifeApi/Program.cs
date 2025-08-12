using FluentValidation;
using IgniteLifeApi.Application.Services.Implementations;
using IgniteLifeApi.Infrastructure.Configuration;
using IgniteLifeApi.Infrastructure.Data;
using IgniteLifeApi.Presentation.Middleware;
using IgniteLifeApi.Presentation.OpenApi.Transformers;
using Microsoft.EntityFrameworkCore;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Services
builder.Services.AddScoped<BookingRuleBlockedPeriodService>();

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddFluentValidationAutoValidation();

// Token service
builder.Services.AddOptions<JwtSettings>()
    .Bind(builder.Configuration.GetSection("JwtSettings"))
    .ValidateDataAnnotations()
    .ValidateOnStart();



// OpenAPI configuration
builder.Services.AddOpenApi(options =>
{
    options.AddOperationTransformer<JsonOnlyOperationTransformer>();
    options.AddOperationTransformer<AuthCookieOperationTransformer>();
    options.AddDocumentTransformer<CookieAuthDocumentTransformer>();
});

var app = builder.Build();

// API Docs
app.MapOpenApi();

// Health checks endpoints
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready");

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();

public partial class Program { }
