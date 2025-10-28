using Microsoft.EntityFrameworkCore;

namespace Core.DataAccess;

public class EntityWriter<T, TDbContext>(TDbContext dbContext) : IEntityWriter<T>
    where T : class
    where TDbContext : DbContext
{
    private readonly TDbContext _dbContext = dbContext;
    private readonly DbSet<T> _set = dbContext.Set<T>();

    public async Task<T> AddAsync(T entity)
    {
        _set.Add(entity);
        await _dbContext.SaveChangesAsync();
        return entity;
    }

    public async Task<T> UpdateAsync(T entity)
    {
        _set.Update(entity);
        await _dbContext.SaveChangesAsync();
        return entity;
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _set.FindAsync(id);
        if (entity is not null)
        {
            _set.Remove(entity);
            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task<T> UpsertAsync(T entity, Func<T, object> keySelector)
    {
        var key = keySelector(entity);
        var existing = await _set.FindAsync(key);

        if (existing is null)
        {
            _set.Add(entity);
        }
        else
        {
            _dbContext.Entry(existing).CurrentValues.SetValues(entity);
        }

        await _dbContext.SaveChangesAsync();
        return entity;
    }

    public async Task<T> UpsertAsync(T entity, params object[] keyValues)
    {
        var existing = await _set.FindAsync(keyValues);

        if (existing is null)
        {
            _set.Add(entity);
        }
        else
        {
            _dbContext.Entry(existing).CurrentValues.SetValues(entity);
        }

        await _dbContext.SaveChangesAsync();
        return entity;
    }
}
