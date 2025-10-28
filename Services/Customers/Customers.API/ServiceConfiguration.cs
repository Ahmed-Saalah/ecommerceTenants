using Customers.Core;
using Customers.Core.DbContexts;
using Customers.Core.Features;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Customers.API;

public static class ServiceConfiguration
{
    public static IServiceCollection ConfigureApplicationService(
        this IServiceCollection svcs,
        IConfiguration config
    )
    {
        svcs.AddCoreServices(config);

        svcs.AddHttpContextAccessor();

        svcs.AddLogging(_ => _.AddConsole().AddDebug());

        return svcs;
    }
}
