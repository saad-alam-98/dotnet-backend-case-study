using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SaudiPlus.Configurations
{
    public static class SwaggerConfiguration
    {
        public static void ConfigureSwaggerExtension(this IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("Mobile", new OpenApiInfo
                {
                    Title = "SaudiPlus Mobile Api",
                    Version = "v1"
                });

                options.SwaggerDoc("Dashboard", new OpenApiInfo
                {
                    Title = "SaudiPlus Dashboard Api",
                    Version = "v1"
                });

                options.SwaggerDoc("Business", new OpenApiInfo
                {
                    Title = "SaudiPlus Business Api",
                    Version = "v1"
                });

                var securityScheme = new OpenApiSecurityScheme
                {
                    Name = "JWT Authentication",
                    Description = "Enter your JWT token in this field",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = JwtBearerDefaults.AuthenticationScheme,
                    BearerFormat = "JWT"
                };

                options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, securityScheme);

                var securityRequirement = new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = JwtBearerDefaults.AuthenticationScheme
                            }
                        },
                        []
                    }
                };
                options.AddSecurityRequirement(securityRequirement);

                options.OperationFilter<LanguageHeaderParameterOperationFilter>();

                options.DocInclusionPredicate((docName, apiDesc) =>
                {
                    var tags = apiDesc.CustomAttributes()
                                    .OfType<SwaggerOperationAttribute>()
                                    .SelectMany(attr => attr.Tags);

                    if (tags.Any())
                    {
                        return tags.Contains(docName);
                    }

                    return docName == "Dashboard" || docName == "Mobile" || docName == "Business";
                });
            });
        }
    }
}
