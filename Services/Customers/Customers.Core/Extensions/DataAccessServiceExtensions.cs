using Core.DataAccess.Abstractions;
using Core.DataAccess.Implementaions;
using Customers.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Customers.Core.Extensions;

public static class DataAccessServiceExtensions
{
    public static IServiceCollection AddDataAccess<TDbContext>(this IServiceCollection services)
        where TDbContext : DbContext
    {
        services.AddScoped<IEntityWriter<Customer>, EntityWriter<Customer, TDbContext>>();
        services.AddScoped<IEntityReader<Customer>, EntityReader<Customer, TDbContext>>();

        services.AddScoped<IEntityWriter<Address>, EntityWriter<Address, TDbContext>>();
        services.AddScoped<IEntityReader<Address>, EntityReader<Address, TDbContext>>();

        return services;
    }
}
