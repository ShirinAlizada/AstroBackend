using AstroBackend.Application.Interfaces.Repositories;
using AstroBackend.Application.Interfaces.Security;
using AstroBackend.Application.Interfaces.Services;
using AstroBackend.Application.Services;
using AstroBackend.Application.Validators;
using AstroBackend.Domain.Entities;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AstroBackend.Application;

public static class ServiceRegistration
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // AuthService IEmailService (şifrə sıfırlama e-poçtu) və sıfırlama linkinin baza
        // URL-ini (appsettings-dəki Frontend:BaseUrl) tələb etdiyi üçün factory ilə qeydiyyatdan
        // keçirilir — IEmailService Infrastructure qatında qeydiyyatdan keçsə də, bu, faktiki
        // resolve zamanı (runtime-da, qeydiyyat ardıcıllığından asılı olmayaraq) həll olunur.
        var frontendBaseUrl = configuration["Frontend:BaseUrl"] ?? "http://localhost:8080";
        services.AddScoped<IAuthService>(sp => new AuthService(
            sp.GetRequiredService<IGenericRepository<User>>(),
            sp.GetRequiredService<IGenericRepository<Profile>>(),
            sp.GetRequiredService<IUnitOfWork>(),
            sp.GetRequiredService<ITokenService>(),
            sp.GetRequiredService<IPasswordHasher>(),
            sp.GetRequiredService<IEmailService>(),
            frontendBaseUrl));

        // Pulsuz (abunəliyi olmayan) istifadəçinin gündəlik AI sorğu limiti — AiLimits:FreeMessagesPerDay
        // (appsettings.json), təyin olunmayıbsa defolt 10.
        var freeAiMessagesPerDay = int.TryParse(configuration["AiLimits:FreeMessagesPerDay"], out var freeLimit) ? freeLimit : 10;
        services.AddScoped<IAiUsageService>(sp => new AiUsageService(
            sp.GetRequiredService<IGenericRepository<AiUsageLog>>(),
            sp.GetRequiredService<IGenericRepository<SubscriptionPlan>>(),
            sp.GetRequiredService<IGenericRepository<UserSubscription>>(),
            sp.GetRequiredService<IUnitOfWork>(),
            freeAiMessagesPerDay));

        services.AddScoped<IProfileService, ProfileService>();
        services.AddScoped<INatalChartService, NatalChartService>();
        services.AddScoped<ISynastryService, SynastryService>();
        services.AddScoped<IHoroscopeService, HoroscopeService>();
        services.AddScoped<IAstrologerService, AstrologerService>();
        services.AddScoped<IBookingService, BookingService>();
        services.AddScoped<IJournalService, JournalService>();
        services.AddScoped<IForumService, ForumService>();
        services.AddScoped<IArticleService, ArticleService>();
        services.AddScoped<IChatService, ChatService>();
        services.AddScoped<INumerologyService, NumerologyService>();
        services.AddScoped<IPanchangService, PanchangService>();
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<IShopService, ShopService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IWishlistService, WishlistService>();
        services.AddScoped<ISubscriptionService, SubscriptionService>();
        services.AddScoped<IContactService, ContactService>();
        services.AddScoped<IPushSubscriptionService, PushSubscriptionService>();

        // FluentValidation — Validators/ qovluğundakı bütün AbstractValidator<T> siniflərini
        // IValidator<T> kimi qeydiyyatdan keçirir. AstroBackend/Filters/ValidationActionFilter.cs
        // bunları action parametrləri üçün avtomatik tapıb tətbiq edir.
        services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();

        return services;
    }
}
