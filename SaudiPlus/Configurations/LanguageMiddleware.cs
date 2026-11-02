using SaudiPlus.Enums;
using SaudiPlus.Interfaces;

namespace SaudiPlus.Configurations
{
    public class LanguageMiddleware
    {
        private readonly RequestDelegate _next;

        public LanguageMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ILanguageService languageService)
        {
            var acceptLanguage = context.Request.Headers["Accept-Language"].ToString();

            if (!string.IsNullOrEmpty(acceptLanguage) && Enum.TryParse<Language>(acceptLanguage, true, out _))
            {
                languageService.Language = acceptLanguage;
            }

            await _next(context);
        }
    }
}
