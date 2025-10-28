using Core.DataAccess.Extensions;
using Customers.Core.DbContexts;
using Customers.Core.Features;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Customers.Core;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCoreServices(
        this IServiceCollection svcs,
        IConfiguration config
    )
    {
        var dbConnectionString =
            config.GetValue<string>("DBConnection")
            ?? throw new Exception("DBConnection configuration not found");

        svcs.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(CreateCustomer.Handler).Assembly)
        );

        svcs.AddDbContext<CustomersDbContext>(options =>
        {
            options.UseSqlServer(dbConnectionString);
        });

        svcs.AddGenericDataAccess<CustomersDbContext>();

        svcs.AddValidatorsFromAssembly(typeof(CreateAddress.Validator).Assembly);

        return svcs;
    }
}
