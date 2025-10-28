using Customers.Core.DbContexts;
using Customers.Core.Extensions;
using Customers.Core.Features;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

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

        svcs.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(CreateCustomer.Handler).Assembly)
        );

        svcs.AddDbContext<CustomersDbContext>(options =>
        {
            options.UseSqlServer(dbConnectionString);
        });

        svcs.AddHttpContextAccessor();

        svcs.AddLogging(_ => _.AddConsole().AddDebug());

        svcs.AddDataAccess<CustomersDbContext>();

        svcs.AddValidatorsFromAssembly(typeof(CreateAddress.Validator).Assembly);

        return svcs;
    }
}
