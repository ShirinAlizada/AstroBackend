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
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Login etdikdən sonra aldığınız AccessToken-i bura yapışdırın (Bearer yazmağa ehtiyac yoxdur)."
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


