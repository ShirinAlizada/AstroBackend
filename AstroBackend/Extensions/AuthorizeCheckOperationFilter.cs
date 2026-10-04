using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AstroBackend.Extensions
{
    /// <summary>
    /// QEYD: Bu filter HAZIRDA `SwaggerExtensions.cs`-də QEYDİYYATDAN keçirilməyib (istifadə
    /// olunmur) — "Authorize" tokeninin Swagger UI-dən göndərilməməsi problemi faktiki olaraq
    /// `SwaggerExtensions.cs`-dəki `AddSecurityRequirement(document => ...)` çağırışında
    /// `OpenApiSecuritySchemeReference("Bearer")`-in `document` arqumentsiz çağırılmasından
    /// qaynaqlanırdı (reference "asılı" qalıb heç nəyə bağlanmırdı) — bu, orada düzəldildi.
    /// Bu sinif operation-səviyyəli (yalnız [Authorize]-lı endpoint-lərə) daha dəqiq həll üçün
    /// saxlanılıb, amma `OperationFilterContext`-in bu Swashbuckle versiyasında document-ə
    /// düzgün bağlanan reference yaratmağa imkan verib-vermədiyi təsdiqlənməyib — ona görə
    /// qeydiyyatdan keçirilməyib. Sınamaq istəsəniz: `SwaggerExtensions.cs`-ə
    /// `c.OperationFilter&lt;AuthorizeCheckOperationFilter&gt;();` əlavə edin və nəticəni yoxlayın.
    /// </summary>
    public class AuthorizeCheckOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var hasAuthorize = context.MethodInfo.DeclaringType is not null &&
                (context.MethodInfo.DeclaringType.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any() ||
                 context.MethodInfo.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any());

            var hasAllowAnonymous = context.MethodInfo.GetCustomAttributes(true).OfType<AllowAnonymousAttribute>().Any();

            if (!hasAuthorize || hasAllowAnonymous)
                return;

            operation.Security ??= [];
            operation.Security.Add(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecuritySchemeReference("Bearer"),
                    new List<string>()
                }
            });
        }
    }
}
