using System.Text;
using AstroBackend.Services;
using AstroBackend.Extensions;
using AstroBackend.Application;
using AstroBackend.Middlewares;
using AstroBackend.Infrastructure;
using Microsoft.IdentityModel.Tokens;
using AstroBackend.Infrastructure.Persistence;
using AstroBackend.Application.Interfaces.Services;
using AstroBackend.Infrastructure.Persistence.SeedData;
using AstroBackend.Application.Interfaces.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

// 1. Onion Architecture Layers Registration
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// 2. Current User & HttpContext
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// 3. JWT Authentication
var jwtSecret = builder.Configuration["JwtSettings:Secret"] ?? "SuperSecretKeyForDestinyReadsAppAstrologyPlatform2026!@#$%^&*()_+";
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"] ?? "DestinyReadsAPI",
        ValidAudience = builder.Configuration["JwtSettings:Audience"] ?? "DestinyReadsClient",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// 4. CORS Policy for Frontend Client
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// 5. Controllers & JSON Options
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

// 6. Swagger Documentation with JWT Authorize support
builder.Services.AddSwaggerDocumentation();

var app = builder.Build();

// 7. Global Exception Handling Middleware
app.UseMiddleware<GlobalExceptionMiddleware>();

// 8. Swagger in Development and Production for easy testing
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Virgo Astrology API v1");
    c.RoutePrefix = string.Empty; // Serves Swagger UI at root (http://localhost:5000/)
});

// 9. CORS & Security Pipeline
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// 10. Database Seeding on Startup
using (var scope = app.Services.CreateScope())
{
    try
    {
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        await DbInitializer.SeedAsync(context, hasher);
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogWarning("SQL Server əlaqəsi xətası: {Message}. (SQL Server işə salındıqda avtomatik qoşulacaq).", ex.Message);
    }
}

app.Run();
