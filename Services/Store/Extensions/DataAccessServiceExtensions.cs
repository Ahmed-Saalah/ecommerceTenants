using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Store.Core.DbContexts;

namespace Store.Core.Extensions;

public static class DataAccessServiceExtensions
{
    public static IServiceCollection AddDataAccess<TDbContext>(this IServiceCollection services)
        where TDbContext : DbContext
    {
        services.AddDataAccess<StoreDbContext>();

        return services;
    }
}
