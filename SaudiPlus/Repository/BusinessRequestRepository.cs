using SaudiPlus.Entities;
using Microsoft.EntityFrameworkCore;
using SaudiPlus.Interfaces.Repository;

namespace SaudiPlus.Repository
{
    public class BusinessRequestRepository : Repository<BusinessRequest, int>, IBusinessRequestRepository
    {
        public BusinessRequestRepository(SaudiPlusDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<BusinessRequest?> GetByBusinessName(string businessName)
        {
            return await _dbContext.BusinessRequest
                .FirstOrDefaultAsync(b => b.BusinessName.ToLower().Trim() == businessName.ToLower().Trim());
        }

        public IQueryable<BusinessRequest> GetRequestForFilter()
        {
            return _dbContext.BusinessRequest
              .OrderByDescending(b => b.CreationTime)
              .AsQueryable();
        }

        public async Task<BusinessRequest?> GetRequestWithRelation(int id)
        {
            return await _dbContext.BusinessRequest
            .FirstOrDefaultAsync(b => b.Id == id);
        }
    }
}
