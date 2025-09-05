using Customers.EventHandlers.Clients.Customers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

await new HostBuilder()
    .ConfigureAppConfiguration(_ =>
        _.AddEnvironmentVariables().AddJsonFile("local.settings.json", true, true)
    )
    .ConfigureServices((context, svcs) => Configure(svcs, context.Configuration))
    .Build()
    .RunAsync();

static void Configure(IServiceCollection svcs, IConfiguration config)
{
    var sbConnectionString =
        config.GetValue<string>("SBConnection")
        ?? throw new System.Exception("SBConnection not found");

    svcs.AddMediatR(_ => _.RegisterServicesFromAssemblyContaining<Program>());

    svcs.AddHttpClient<ICustomersClient, CustomersClient>(_ =>
    {
        _.BaseAddress = new Uri(
            config.GetValue<string>("CustomersApiUrl")
                ?? throw new Exception("Customers Api Url not exist in config")
        );
    });
}
