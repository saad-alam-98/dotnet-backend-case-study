using SaudiPlus.Entities;
using Microsoft.EntityFrameworkCore;
using SaudiPlus.Interfaces.Repository;

namespace SaudiPlus.Repository
{
    public class ContractRepository : Repository<Contract, int>, IContractRepository
    {
        public ContractRepository(SaudiPlusDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<Contract?> GetBusinessContract(Guid businessId)
        {
           return await _dbContext
                .Contract
                .FirstOrDefaultAsync(c => c.BusinessId == businessId);
        }
    }
}
