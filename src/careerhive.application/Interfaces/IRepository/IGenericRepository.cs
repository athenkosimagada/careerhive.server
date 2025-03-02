using System.Linq.Expressions;
using System.Threading.Tasks;

namespace careerhive.application.Interfaces.IRepository;

public interface IGenericRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id, params Expression<Func<T, object>>[] includes);
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes);
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task AddRangeAsync(IEnumerable<T> entities);
    Task RemoveAsync(T entity);
    Task RemoveRangeAsync(IEnumerable<T> entities);

    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);

    Task<IEnumerable<T>> GetPagedAsync(int pageNumber, int pageSize, 
        Expression<Func<T, object>>? orderBy = null, bool descending = false,
        Expression<Func<T, bool>>? where = null,
        params Expression<Func<T, object>>[] includes);
}
