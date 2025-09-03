﻿using System.Reflection;
using Auth.API.Extensions;

namespace Customers.API.Extensions;

public static class EndpointExtensions
{
    public static void MapFeatureEndpoints(this IEndpointRouteBuilder app)
    {
        var endpointTypes = Assembly
            .GetExecutingAssembly()
            .GetTypes()
            .Where(t =>
                typeof(IEndpoint).IsAssignableFrom(t)
                && t is { IsAbstract: false, IsInterface: false }
            );

        foreach (var type in endpointTypes)
        {
            var endpoint = (IEndpoint)Activator.CreateInstance(type)!;
            endpoint.Map(app);
        }
    }
}
