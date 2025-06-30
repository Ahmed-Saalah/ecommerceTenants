using Auth.API.Models;
using Core.DataAccess.Abstractions;
using Core.DataAccess.Implementaions;
using Microsoft.EntityFrameworkCore;

namespace Auth.API.Extensions;

public static class DataAccessServiceExtensions
{
    public static IServiceCollection AddDataAccess<TDbContext>(this IServiceCollection services)
        where TDbContext : DbContext
    {
        services.AddScoped<IEntityWriter<UserTenant>, EntityWriter<UserTenant, TDbContext>>();
        services.AddScoped<IEntityReader<UserTenant>, EntityReader<UserTenant, TDbContext>>();

        return services;
    }
}
