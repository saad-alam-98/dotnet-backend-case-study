using SaudiPlus.Entities;
using Microsoft.EntityFrameworkCore;
using SaudiPlus.Interfaces.Repository;

namespace SaudiPlus.Repository
{
    public class StatusRepository : Repository<Status, int>, IStatusRepository
    {
        public StatusRepository(SaudiPlusDbContext dbContext) : base(dbContext)
        {
        }

        public IQueryable<Status> GetActiveStatus()
        {
            var now = DateTime.UtcNow;

            return _dbContext.Status
                .Include(s => s.Business)
                .Include(s => s.Offer)
                .Where(s => s.IsActive
                         && !s.IsDelete
                         && now <= s.CreationTime.AddSeconds(s.Duration))
                .OrderByDescending(s => s.CreationTime);
        }

        public IQueryable<Status> GetAdminStatus()
        {
            return _dbContext.Status
               .Include(s => s.Business)
               .Include(s => s.Offer)
               .OrderByDescending(s => s.CreationTime)
               .Where(s => s.FromAdmin)
               .AsQueryable();
        }

        public IQueryable<Status> GetBusinessStatus(Guid businessId)
        {
            return _dbContext.Status
               .Include(s => s.Business)
               .Include(s => s.Offer)
               .OrderByDescending(s => s.CreationTime)
               .Where(s => !s.FromAdmin && s.BusinessId == businessId)
               .AsQueryable();
        }

        public async Task<Status?> GetStatusWithRelation(int id)
        {
            return await _dbContext.Status
                .Include(s => s.Business)
                .Include(s => s.Offer)
                .FirstOrDefaultAsync(x => x.Id == id);
        }
    }
}