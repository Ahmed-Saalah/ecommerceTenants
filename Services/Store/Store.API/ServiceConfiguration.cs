using Store.Core.Extensions;

namespace Store.API;

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
