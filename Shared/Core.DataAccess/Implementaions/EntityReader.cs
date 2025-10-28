using System.Linq.Expressions;
using Core.DataAccess.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Core.DataAccess;

public class EntityReader<T, TDbContext>(TDbContext dbContext) : IEntityReader<T>
    where T : class
    where TDbContext : DbContext
{
    private readonly DbSet<T> _set = dbContext.Set<T>();

    public async Task<T?> GetByIdAsync(int id) => await _set.FindAsync(id);

    public async Task<IEnumerable<T>> GetAllAsync() => await _set.AsNoTracking().ToListAsync();

    public async Task<T?> GetOneByAsync(Expression<Func<T, bool>> predicate)
    {
        return await _set.AsNoTracking().FirstOrDefaultAsync(predicate);
    }

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate) =>
        await _set.Where(predicate).ToListAsync();

    public async Task<int> CountAsync() => await _set.CountAsync();

    public async Task<int> CountAsync(Expression<Func<T, bool>> predicate)
    {
        return await _set.CountAsync(predicate);
    }

    public async Task<PagedResult<T>> GetPagedAsync(int pageNumber, int pageSize)
    {
        var totalCount = await _set.CountAsync();
        var items = await _set.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PagedResult<T>
        {
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
            Items = items,
        };
    }
}
