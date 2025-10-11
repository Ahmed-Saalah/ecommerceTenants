namespace Core.DataAccess.Abstractions;

public interface IEntityWriter<T>
    where T : class
{
    Task<T> AddAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task DeleteAsync(int id);
    Task<T> UpsertAsync(T entity, Func<T, object> keySelector);
    Task<T> UpsertAsync(T entity, params object[] keyValues);
}
