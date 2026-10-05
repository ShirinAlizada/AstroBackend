using AstroBackend.Application.Interfaces.Repositories;
using AstroBackend.Application.Interfaces.Security;
using AstroBackend.Application.Interfaces.Services;
using AstroBackend.Infrastructure.Persistence;
using AstroBackend.Infrastructure.Persistence.Repositories;
using AstroBackend.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AstroBackend.Infrastructure
{
    public static class ServiceRegistration
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration, string contentRootPath)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
               
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(connectionString));

            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IPasswordHasher, PasswordHasherService>();
            services.AddScoped<ITokenService, JwtTokenService>();

            // AI Service HTTP Client Registration
            services.AddHttpClient<IAIService, GeminiAIService>();

            services.AddScoped<IFileStorageService>(_ => new LocalFileStorageService(contentRootPath));

            // Web Push (VAPID) — açarlar appsettings.json-dakı Vapid bölməsindən.
            services.AddScoped<IPushService>(_ => new WebPushService(
                configuration["Vapid:PublicKey"] ?? string.Empty,
                configuration["Vapid:PrivateKey"] ?? string.Empty,
                configuration["Vapid:Subject"] ?? "mailto:admin@virgoastrology.local"));

            // SMTP e-poçt — konfiqurasiya appsettings.json-dakı Smtp bölməsindən.
            services.AddScoped<IEmailService>(_ => new SmtpEmailService(
                configuration["Smtp:Host"],
                int.TryParse(configuration["Smtp:Port"], out var smtpPort) ? smtpPort : 587,
                configuration["Smtp:User"],
                configuration["Smtp:Password"],
                configuration["Smtp:From"] ?? "no-reply@virgoastrology.local",
                configuration["Smtp:FromName"] ?? "Virgo Astrology",
                bool.TryParse(configuration["Smtp:EnableSsl"], out var enableSsl) && enableSsl));

            return services;
        }
    }
}
