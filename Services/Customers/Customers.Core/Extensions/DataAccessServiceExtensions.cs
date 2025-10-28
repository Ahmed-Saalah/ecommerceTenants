using Customers.Core.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Customers.Core.Extensions;

public static class DataAccessServiceExtensions
{
    public static IServiceCollection AddDataAccess<TDbContext>(this IServiceCollection services)
        where TDbContext : DbContext
    {
        services.AddDataAccess<CustomersDbContext>();

        return services;
    }
}
