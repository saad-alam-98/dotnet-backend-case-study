using SaudiPlus.Enums;
using SaudiPlus.Entities;
using Microsoft.EntityFrameworkCore;
using SaudiPlus.Interfaces.Repository;

namespace SaudiPlus.Repository
{
    public class BusinessPolicyRepository : Repository<BusinessPolicy, int>, IBusinessPolicyRepository
    {
        public BusinessPolicyRepository(SaudiPlusDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<BusinessPolicy?> GetBusinessPolicyByType(Guid BusinessId, PolicyType type)
        {
            return await _dbContext.BusinessPolicy.FirstOrDefaultAsync(p => p.Type == type && p.BusinessId == BusinessId);
        }
    }
}
