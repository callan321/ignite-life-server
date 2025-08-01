using IgniteLifeApi.Data;
using IgniteLifeApi.Services;
using Microsoft.EntityFrameworkCore;
using Server.Services;

var builder = WebApplication.CreateBuilder(args);

// ✅ Health checks now use built-in support (no NuGet package needed)
builder.Services.AddHealthChecks();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddOpenApi(); // For OpenAPI/Swagger

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Services
builder.Services.AddScoped<BookingRulesService>();
builder.Services.AddScoped<BookingRuleOpeningExceptionService>();
builder.Services.AddScoped<BookingServiceTypeService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// ✅ Run EF migrations automatically on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

// ✅ Health checks endpoints
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready");

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();

public partial class Program { }
