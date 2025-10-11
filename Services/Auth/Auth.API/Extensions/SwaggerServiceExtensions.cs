using Microsoft.OpenApi.Models;

namespace Auth.API.Extensions;

public static class SwaggerServiceExtensions
{
    public static IServiceCollection AddSwaggerServices(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(setupAction =>
        {
            setupAction.AddSecurityDefinition(
                "CloudCommerceApiBearerAuth",
                new OpenApiSecurityScheme()
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    Description = "Input a valid token to access this API",
                }
            );

            setupAction.CustomSchemaIds(opts => opts.FullName?.Replace("+", "."));

            setupAction.AddSecurityRequirement(
                new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "CloudCommerceApiBearerAuth",
                            },
                        },
                        new List<string>()
                    },
                }
            );
        });

        return services;
    }
}
