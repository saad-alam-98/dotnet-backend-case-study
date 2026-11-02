using SaudiPlus.Entities;
using Microsoft.EntityFrameworkCore;
using SaudiPlus.Interfaces.Repository;

namespace SaudiPlus.Repository
{
    public class SubAdminRepository : Repository<SubAdminUser, Guid>, ISubAdminRepository
    {
        public SubAdminRepository(SaudiPlusDbContext dbContext) : base(dbContext)
        {
        }

        public IQueryable<SubAdminUser> GetSubAdminsForFilter()
        {
            return _dbContext.SubAdminUser
                .OrderByDescending(b => b.CreationTime)
                .AsQueryable();
        }

        public async Task<SubAdminUser?> GetSubAdminUserByEmail(string email)
        {
            return await _dbContext.SubAdminUser
                .Include(p => p.Permissions)
                .FirstOrDefaultAsync(p => p.Email == email);
        }

        public async Task<SubAdminUser?> GetSubAdminUserByRelation(Guid id)
        {
            return await _dbContext.SubAdminUser
                .Include(p => p.Permissions)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<SubAdminUser?> GetSubAdminUserByUserId(Guid id)
        {
            return await _dbContext.SubAdminUser
                .Include(p => p.Permissions)
                .FirstOrDefaultAsync(p => p.UserId == id);
        }
    }
}
