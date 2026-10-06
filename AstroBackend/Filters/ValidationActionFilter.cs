using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AstroBackend.Filters
{
    /// <summary>
    /// Hər bir action parametri üçün — əgər həmin tip üçün DI-da bir IValidator&lt;T&gt;
    /// qeydiyyatdan keçibsə (bax: AstroBackend.Application/Validators/) — FluentValidation ilə
    /// doğrulama aparan qlobal MVC filter-i. FluentValidation-ın köhnəlmiş "avtomatik ASP.NET Core
    /// inteqrasiyası" (FluentValidation.AspNetCore-un AddFluentValidation() üsulu) paketinin
    /// əvəzinə yazılıb — yalnız core FluentValidation + DependencyInjectionExtensions
    /// paketlərindən (hər ikisi artıq layihədə referans edilib) istifadə edir.
    ///
    /// Bir tip üçün validator qeydiyyatdan keçməyibsə, bu filter heç nə etmir — mövcud davranış
    /// dəyişmir. Yalnız Validators/ qovluğuna əlavə edilən DTO tipləri üçün aktivdir.
    /// </summary>
    public class ValidationActionFilter : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            foreach (var arg in context.ActionArguments.Values)
            {
                if (arg == null) continue;

                var validatorType = typeof(IValidator<>).MakeGenericType(arg.GetType());
                if (context.HttpContext.RequestServices.GetService(validatorType) is not IValidator validator)
                    continue;

                var validationContext = new ValidationContext<object>(arg);
                var result = await validator.ValidateAsync(validationContext, context.HttpContext.RequestAborted);

                if (!result.IsValid)
                {
                    foreach (var error in result.Errors)
                        context.ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                }
            }

            if (!context.ModelState.IsValid)
            {
                context.Result = new BadRequestObjectResult(new ValidationProblemDetails(context.ModelState));
                return;
            }

            await next();
        }
    }
}
