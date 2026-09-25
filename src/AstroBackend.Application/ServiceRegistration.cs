using AstroBackend.Application.Interfaces.Services;
using AstroBackend.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AstroBackend.Application;

public static class ServiceRegistration
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
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

        return services;
    }
}
