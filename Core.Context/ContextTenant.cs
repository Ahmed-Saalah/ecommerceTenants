using Microsoft.AspNetCore.Mvc;

namespace Core.Context;

public class ContextTenant
{
    [FromHeader(Name = ContextHttpHeaders.Tenant)]
    public string? HeaderValue { get; set; }

    [FromRoute(Name = "tenantId")]
    public string? RouteValue { get; set; }

    public static implicit operator int(ContextTenant source)
    {
        if (!string.IsNullOrEmpty(source.RouteValue) && source.RouteValue.ToLower() != "me")
        {
            return int.Parse(source.RouteValue);
        }

        return int.Parse(source.HeaderValue!);
    }

    public static implicit operator string(ContextTenant source)
    {
        if (!string.IsNullOrEmpty(source.RouteValue) && source.RouteValue.ToLower() != "me")
        {
            return source.RouteValue!;
        }

        return source.HeaderValue!;
    }
}
