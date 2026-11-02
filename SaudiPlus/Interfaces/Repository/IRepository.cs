using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace SaudiPlus.Interfaces.Repository
{
    public interface IRepository<TEntity, TId> where TEntity : Entity<TId>
    {
        Task<IEnumerable<TEntity>> GetAsync();
        Task<TEntity> GetByIdAsync(TId id);
        Task<TEntity> FindAsync([NotNull] TId id);
        Task InsertAsync([NotNull] TEntity entity, bool autoSave = false, CancellationToken cancellationToken = default);
        Task InsertAsync([NotNull] TEntity[] entity, bool autoSave = false, CancellationToken cancellationToken = default);
        Task DeleteAsync([NotNull] TId id, bool autoSave, CancellationToken cancellationToken = default);
        Task DeleteAsync([NotNull] TEntity[] entity, bool autoSave, CancellationToken cancellationToken = default);
        Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate);
        Task UpdateAsync(TEntity model, bool autoSave = false, CancellationToken cancellationToken = default);
    }
}
