using Customers.API.DbContexts;
using Customers.API.Extensions;
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
        var dbConnectionString =
            config.GetValue<string>("DBConnection")
            ?? throw new Exception("DBConnection configuration not found");

        var sbConnectionString =
            config.GetValue<string>("SBConnection")
            ?? throw new Exception("SBConnection configuration not found");

        svcs.AddMediatR(_ => _.RegisterServicesFromAssemblyContaining<Program>());

        svcs.AddDbContext<CustomersDbContext>(options =>
        {
            options.UseSqlServer(dbConnectionString);
        });

        svcs.AddHttpContextAccessor();

        svcs.AddLogging(_ => _.AddConsole().AddDebug());

        svcs.AddTransient<ILogger>(_ => _.GetRequiredService<ILogger<Program>>());

        svcs.AddDataAccess<CustomersDbContext>();

        svcs.AddHttpContextAccessor();

        svcs.AddValidatorsFromAssembly(typeof(Program).Assembly);

        return svcs;
    }
}
