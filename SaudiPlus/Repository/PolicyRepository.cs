using SaudiPlus.Enums;
using SaudiPlus.Entities;
using Microsoft.EntityFrameworkCore;
using SaudiPlus.Interfaces.Repository;

namespace SaudiPlus.Repository
{
    public class PolicyRepository : Repository<Policy, int>, IPolicyRepository
    {
        public PolicyRepository(SaudiPlusDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<Policy?> GetPolicyByType(PolicyType type)
        {
            return await _dbContext.Policy.FirstOrDefaultAsync(p => p.Type == type);
        }
    }
}