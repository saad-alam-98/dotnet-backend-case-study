using SaudiPlus.Entities;
using Microsoft.EntityFrameworkCore;
using SaudiPlus.Interfaces.Repository;

namespace SaudiPlus.Repository
{
    public class UserPermissionRepository : Repository<UserPermission, int>, IUserPermissionRepository
    {
        public UserPermissionRepository(SaudiPlusDbContext dbContext) : base(dbContext)
        {
            
        }

        public async Task DeleteByUserIdAsync(Guid userId)
        {
            var existing = await _dbContext.UserPermissions
            .Where(p => p.UserId == userId)
            .ToArrayAsync();

            await DeleteAsync(existing, true);
        }

        public async Task<List<UserPermission>> GetByUserIdAsync(Guid userId)
        {
            return await _dbContext.UserPermissions
                .Where(p => p.UserId == userId)
                .ToListAsync();
        }
    }
}
