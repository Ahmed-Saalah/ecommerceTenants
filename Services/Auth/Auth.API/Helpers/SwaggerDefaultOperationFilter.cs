using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Auth.API.Helpers;

/// <summary>
/// Add default parameters / headers that are applicablt to all apis
/// </summary>
internal class SwaggerDefaultOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.Parameters == null)
            operation.Parameters = new List<OpenApiParameter>();

        operation.Parameters.Add(Header("X-Lang", "UI Language", defaultValue: "en"));
        operation.Parameters.Add(Header("X-Tenant", "Tenant Id"));
    }

    OpenApiParameter Header(
        string name,
        string description,
        string schemaType = "string",
        string? defaultValue = null,
        bool required = false
    ) =>
        new OpenApiParameter
        {
            Name = name,
            In = ParameterLocation.Header,
            Description = description,
            Required = required,
            Schema = new OpenApiSchema
            {
                Type = schemaType,
                Default = defaultValue != null ? new OpenApiString(defaultValue) : null,
            },
        };
}
