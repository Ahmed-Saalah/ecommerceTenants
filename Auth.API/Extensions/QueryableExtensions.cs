using Auth.API.Models.Constants;
using Microsoft.EntityFrameworkCore;

namespace Auth.API.Extensions;

public static class QueryableExtensions
{
    public static IQueryable<T> ForTenant<T>(this DbSet<T> dbSet, int tenantId)
        where T : class, ITenantOwned
    {
        return dbSet.Where(e => e.TenantId == tenantId);
    }
}
