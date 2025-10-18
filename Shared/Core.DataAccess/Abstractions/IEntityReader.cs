using System.Linq.Expressions;
using Core.DataAccess.Helpers;

namespace Core.DataAccess.Abstractions;

public interface IEntityReader<T>
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<T?> GetOneByAsync(Expression<Func<T, bool>> predicate);
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<int> CountAsync();
    Task<int> CountAsync(Expression<Func<T, bool>> predicate);
    Task<PagedResult<T>> GetPagedAsync(int pageNumber, int pageSize);
}
