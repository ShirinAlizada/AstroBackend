using Microsoft.OpenApi;

namespace AstroBackend.Extensions
{
    public static class SwaggerExtensions
    {
        public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Virgo Astrology (Ruh Astrolojiya) Web API",
                    Version = "v1",
                    Description = "Onion Architecture ilə qurulmuş Astrologiya və Doğum Xəritəsi Platforması REST API."
                });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization başlığı 'Bearer' sxemindən istifadə edir. Nümunə: 'Bearer 12345abcdef'",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
{
    {
        new OpenApiSecuritySchemeReference("Bearer"),
        new List<string>()
    }
});
            });

            return services;
        }
    }
}


