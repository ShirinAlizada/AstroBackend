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

                // DİQQƏT: "Bearer" reference-i document kontekstinə bağlanmalıdır, əks halda
                // (yəni new OpenApiSecuritySchemeReference("Bearer") — document arqumenti olmadan)
                // reference "asılı" qalır və Swagger UI "Authorize"dan sonra belə tokeni faktiki
                // sorğuya əlavə etmir (biz məhz bu səbəbdən 401 alırdıq). Microsoft.OpenApi v2 +
                // Swashbuckle.AspNetCore 10.x üçün rəsmi düzgün forma budur:
                c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
                {
                    {
                       new OpenApiSecuritySchemeReference("Bearer", document),
                       new List<string>()
                    }
                });
            });

            return services;
        }
    }
}


