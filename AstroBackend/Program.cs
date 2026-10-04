using AstroBackend.Application;
using AstroBackend.Application.Interfaces.Security;
using AstroBackend.Application.Interfaces.Services;
using AstroBackend.Extensions;
using AstroBackend.Infrastructure;
using AstroBackend.Infrastructure.Persistence;
using AstroBackend.Infrastructure.Persistence.SeedData;
using AstroBackend.Middlewares;
using AstroBackend.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 1. Onion Architecture Layers Registration
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration, builder.Environment.ContentRootPath);

// 2. Current User & HttpContext
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// 3. JWT Authentication & Role Configuration
var jwtSecret = builder.Configuration["JwtSettings:Secret"];
if (string.IsNullOrWhiteSpace(jwtSecret) || jwtSecret.Length < 32)
{
    throw new InvalidOperationException(
        "JwtSettings:Secret appsettings.json-da (və ya mühit dəyişənlərində) təyin edilməyib, " +
        "yaxud 32 simvoldan qısadır. Tokenləri etibarlı şəkildə imzalamaq üçün güclü, " +
        "təsadüfi generasiya olunmuş bir açar təyin edin — sərt kodlanmış defolt açar istifadə edilmir.");
}
var jwtIssuer = builder.Configuration["JwtSettings:Issuer"] ?? "DestinyReadsAPI";
var jwtAudience = builder.Configuration["JwtSettings:Audience"] ?? "DestinyReadsClient";

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
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        RoleClaimType = ClaimTypes.Role,
        NameClaimType = ClaimTypes.NameIdentifier,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// 4. CORS Policy for Frontend Client
// Mənbələr appsettings.json-dakı Cors:AllowedOrigins-dən oxunur; təyin olunmayıbsa,
// yalnız lokal development mənbələrinə (frontend dev-server) icazə verilir. Production-da
// real frontend domenini appsettings(.Production).json-a əlavə etmək lazımdır.
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? new[] { "http://localhost:8080", "http://localhost:3000", "http://localhost:5173" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowedOrigins", policy =>
    {
        policy.WithOrigins(allowedOrigins)
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

// 8. Swagger — yalnız Development mühitində açıqdır (production-da API sxemini və
// bütün endpoint-ləri ictimai şəkildə açmamaq üçün).
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Destiny Reads API v1");
        c.RoutePrefix = string.Empty; // Serves Swagger UI at root
    });
}

// 9. CORS & Security Pipeline
app.UseCors("AllowedOrigins");
app.UseStaticFiles(); // wwwroot/uploads (avatarlar və s.) statik fayl kimi ötürülür
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
