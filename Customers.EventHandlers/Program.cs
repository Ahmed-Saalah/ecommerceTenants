using Customers.EventHandlers;
using Customers.EventHandlers.Clients.Customers;
using Customers.EventHandlers.Handlers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureAppConfiguration(config =>
    {
        config.AddJsonFile("local.settings.json", optional: true, reloadOnChange: true);
        config.AddEnvironmentVariables();
    })
    .ConfigureServices(
        (context, services) =>
        {
            Configure(services, context.Configuration);
        }
    )
    .Build();

await host.RunAsync();

static void Configure(IServiceCollection services, IConfiguration config)
{
    services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Program>());
    services.AddSingleton<IEventHandler, UserCreatedEventHandler>();

    services.AddHttpClient<ICustomersClient, CustomersClient>(client =>
    {
        client.BaseAddress = new Uri(
            config.GetValue<string>("CustomersApiUrl")
                ?? throw new Exception("CustomersApiUrl not found in config")
        );
    });
}
