using System.Globalization;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Localization;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SaudiPlus.Configurations
{
    public static class LanguageConfiguration
    {
        public static void ConfigureLanguageService(this IServiceCollection services)
        {
            services.AddLocalization(
                option =>
                {
                    option.ResourcesPath = "Resources";
                }
            );

            services.Configure<RequestLocalizationOptions>(
                option =>
                {
                    var SupportedCulturs = new List<CultureInfo> {
                        new("ar"),
                        new("en")
                    };

                    option.DefaultRequestCulture = new RequestCulture("ar", "ar");
                    option.SupportedCultures = SupportedCulturs;
                    option.SupportedUICultures = SupportedCulturs;

                    option.RequestCultureProviders =
                    [
                        new HeaderRequestCultureProvider{
                            HeaderKey = "Accept-Language"
                        }
                    ];
                }
            );
        }

        public class HeaderRequestCultureProvider : IRequestCultureProvider
        {
            public string HeaderKey { get; set; } = "Accept-Language";
            string[] Languages = ["ar", "en"];

            public Task<ProviderCultureResult?> DetermineProviderCultureResult(HttpContext httpContext)
            {
                var culture = httpContext.Request.Headers[HeaderKey].ToString();
                if (string.IsNullOrEmpty(culture) || !Languages.Contains(culture))
                {
                    culture = "ar";
                }

                var result = new ProviderCultureResult(culture);
                return Task.FromResult(result);
            }
        }
    }
    public class LanguageHeaderParameterOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "Accept-Language",
                In = ParameterLocation.Header,
                Description = "Specify the language code ('en' or 'ar')",
                Required = false,
                Schema = new OpenApiSchema
                {
                    Type = "string",
                    Default = new OpenApiString("ar")
                }
            });
        }
    }
}
