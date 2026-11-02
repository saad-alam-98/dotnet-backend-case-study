using SaudiPlus.Entities;
using Microsoft.EntityFrameworkCore;
using SaudiPlus.Interfaces.Repository;

namespace SaudiPlus.Repository
{
    public class ForgetPasswordRepository : Repository<ForgetPassword, int>, IForgetPasswordRepository
    {
        public ForgetPasswordRepository(SaudiPlusDbContext db) : base(db)
        {
           
        }

        public async Task<ForgetPassword?> FindByUserIdAsync(Guid userId)
        {
            if (userId == Guid.Empty)
            {
                return null;
            }

            return await _dbContext.Set<ForgetPassword>()
                .Where(fp => fp.UserId == userId)
                .OrderByDescending(fp => fp.ExpiryTime)
                .FirstOrDefaultAsync();
        }
    }
}
