using AstroBackend.Application;
using AstroBackend.Application.Interfaces.Security;
using AstroBackend.Application.Interfaces.Services;
using AstroBackend.Extensions;
using AstroBackend.Filters;
using AstroBackend.HealthChecks;
using AstroBackend.Infrastructure;
using AstroBackend.Infrastructure.Persistence;
using AstroBackend.Infrastructure.Persistence.SeedData;
using AstroBackend.Middlewares;
using AstroBackend.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// 0. Serilog — strukturlaşdırılmış loglama (Console + gündəlik fayl). appsettings.json-dakı
// "Serilog" bölməsindən oxunur; builder-in daxili ILogger<T> istifadəsini şəffaf şəkildə əvəz edir.
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .Enrich.FromLogContext());

// 1. Onion Architecture Layers Registration
builder.Services.AddApplicationServices(builder.Configuration);
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
builder.Services.AddControllers(options =>
    {
        // FluentValidation-ı bütün action-lar üçün avtomatik tətbiq edir (DTO validasiyası) —
        // bax: AstroBackend/Filters/ValidationActionFilter.cs.
        options.Filters.Add<ValidationActionFilter>();
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

// 6. Swagger Documentation with JWT Authorize support
builder.Services.AddSwaggerDocumentation();

// 6b. Rate Limiting — spam/abuse qorunması (ASP.NET Core daxili middleware, əlavə paket tələb etmir).
// "contact" siyasəti: IP üzrə saatda 3 sorğu (Əlaqə formu, ContactController-də [EnableRateLimiting]).
// "ai" siyasəti: istifadəçi üzrə (NameIdentifier claim-i ilə partitioned) dəqiqədə 10 sorğu —
// Gemini çağırışlarının pul xərcini məhdudlaşdırmaq üçün (AIController-də [EnableRateLimiting]).
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("contact", opt =>
    {
        opt.PermitLimit = 3;
        opt.Window = TimeSpan.FromHours(1);
        opt.QueueLimit = 0;
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });
    options.AddPolicy("ai", httpContext =>
    {
        var userKey = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? httpContext.Connection.RemoteIpAddress?.ToString()
            ?? "anonymous";

        return RateLimitPartition.GetFixedWindowLimiter(userKey, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 10,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst
        });
    });
});

// 6c. Health Checks — yükləmə balanslayıcıları/monitorinq üçün /health endpoint-i.
builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("database");

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
app.UseStaticFiles(); // wwwroot/uploads (avatarlar, mağaza şəkilləri və s.) statik fayl kimi ötürülür
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.MapControllers();
app.MapHealthChecks("/health");

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
