using Core.DataAccess.Extensions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Store.Core.DbContexts;
using Store.Core.Features;

namespace Store.Core.Extensions;

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
            cfg.RegisterServicesFromAssembly(typeof(CreateStore.Handler).Assembly)
        );

        svcs.AddDbContext<StoreDbContext>(options =>
        {
            options.UseSqlServer(dbConnectionString);
        });

        svcs.AddGenericDataAccess<StoreDbContext>();

        svcs.AddValidatorsFromAssembly(typeof(CreateStore.Validator).Assembly);

        return svcs;
    }
}
