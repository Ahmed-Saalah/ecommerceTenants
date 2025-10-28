using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Core.DataAccess.Extensions;

public static class DataAccessServiceExtensions
{
    public static IServiceCollection AddGenericDataAccess<TDbContext>(
        this IServiceCollection services
    )
        where TDbContext : DbContext
    {
        var writerInterface = typeof(IEntityWriter<>);
        var writerImplementation = typeof(EntityWriter<,>);
        var readerInterface = typeof(IEntityReader<>);
        var readerImplementation = typeof(EntityReader<,>);

        var dbSetTypes = typeof(TDbContext)
            .GetProperties()
            .Where(p =>
                p.PropertyType.IsGenericType
                && p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>)
            )
            .Select(p => p.PropertyType.GetGenericArguments()[0]);

        foreach (var entityType in dbSetTypes)
        {
            var writerServiceType = writerInterface.MakeGenericType(entityType);
            var writerImplementationType = writerImplementation.MakeGenericType(
                entityType,
                typeof(TDbContext)
            );
            var readerServiceType = readerInterface.MakeGenericType(entityType);
            var readerImplementationType = readerImplementation.MakeGenericType(
                entityType,
                typeof(TDbContext)
            );
            services.AddScoped(writerServiceType, writerImplementationType);
            services.AddScoped(readerServiceType, readerImplementationType);
        }

        return services;
    }
}
