using SaudiPlus.Entities;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SaudiPlus.Interfaces.Repository;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Storage;

namespace SaudiPlus.Repository
{
    public class Repository<TEntity, TId> : IRepository<TEntity, TId>
        where TEntity : Entity<TId>
    {
        protected SaudiPlusDbContext _dbContext;
        private IDbContextTransaction? _dbTransaction;
        public Repository(SaudiPlusDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<TEntity>> GetAsync()
        {
            var data = await _dbContext.Set<TEntity>().ToListAsync();
            return data;
        }

        public async Task<TEntity> GetByIdAsync(TId id)
        {
            var entity = await _dbContext.Set<TEntity>().FindAsync(id);
            return entity;
        }

        public async Task<TEntity> FindAsync([NotNull] TId id)
        {
            var entity = await _dbContext.Set<TEntity>().FindAsync(id);
            return entity;
        }

        public async Task InsertAsync([NotNull] TEntity entity, bool autoSave = false, CancellationToken cancellationToken = default)
        {
            await _dbContext.Set<TEntity>().AddAsync(entity);
            if (autoSave)
                await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task InsertAsync([NotNull] TEntity[] entity, bool autoSave = false, CancellationToken cancellationToken = default)
        {
            await _dbContext.Set<TEntity>().AddRangeAsync(entity);
            if (autoSave)
                await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync([NotNull] TId id, bool autoSave = false, CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<TEntity>().FindAsync(id);
            _dbContext.Set<TEntity>().Remove(entity);
            if (autoSave)
                await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync([NotNull] TEntity[] entity, bool autoSave = false, CancellationToken cancellationToken = default)
        {
            _dbContext.Set<TEntity>().RemoveRange(entity);
            if (autoSave)
                await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await _dbContext.Set<TEntity>().AnyAsync(predicate);
        }

        public async Task UpdateAsync(TEntity model, bool autoSave = false, CancellationToken cancellationToken = default)
        {
            _dbContext.Set<TEntity>().Update(model);
            if (autoSave)
                await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
