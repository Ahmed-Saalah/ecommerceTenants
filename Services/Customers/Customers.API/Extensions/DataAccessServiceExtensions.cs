using Core.DataAccess.Abstractions;
using Core.DataAccess.Implementaions;
using Customers.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Customers.API.Extensions;

public static class DataAccessServiceExtensions
{
    public static IServiceCollection AddDataAccess<TDbContext>(this IServiceCollection services)
        where TDbContext : DbContext
    {
        services.AddScoped<IEntityWriter<Customer>, EntityWriter<Customer, TDbContext>>();
        services.AddScoped<IEntityReader<Customer>, EntityReader<Customer, TDbContext>>();

        services.AddScoped<IEntityWriter<Address>, EntityWriter<Address, TDbContext>>();
        services.AddScoped<IEntityReader<Address>, EntityReader<Address, TDbContext>>();

        services.AddScoped<
            IEntityWriter<CustomerTenant>,
            EntityWriter<CustomerTenant, TDbContext>
        >();
        services.AddScoped<
            IEntityReader<CustomerTenant>,
            EntityReader<CustomerTenant, TDbContext>
        >();

        return services;
    }
}
