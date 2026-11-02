using SaudiPlus.Entities;
using Microsoft.EntityFrameworkCore;
using SaudiPlus.Interfaces.Repository;

namespace SaudiPlus.Repository
{
    public class BranchRepository : Repository<Branch, int>, IBranchRepository
    {
        public BranchRepository(SaudiPlusDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<List<Branch>> GetBranchWithRelations()
        {
            return await _dbContext.Branch
                .Include(b => b.Business)
                .Include(b => b.City)
                .OrderByDescending(b => b.CreationTime)
                .ToListAsync();
        }

        public async Task<Branch?> GetBranchWithRelations(int id)
        {
            return await _dbContext.Branch
                .Include(b => b.Business)
                .Include(b => b.City)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<List<Branch>> GetBusinessBranches(Guid businessId)
        {
            return await _dbContext.Branch
                .Include(b => b.Business)
                .Include(b => b.City)
                .Where(b => b.BusinessId == businessId)
                .OrderByDescending(b => b.CreationTime)
                .ToListAsync();
        }

        public async Task<Branch?> GetBranchByBranchNumber(int BranchNumber)
        {
            return await _dbContext.Branch
            .FirstOrDefaultAsync(b => b.BranchNumber == BranchNumber);
        }

        public IQueryable<Branch> GetBranchForFilter(Guid businessId)
        {
            return _dbContext.Branch
              .Where(b => b.BusinessId == businessId)
                .Include(b => b.Business)
                    .ThenInclude(b => b.Category)
                .Include(b => b.City)
              .OrderByDescending(b => b.CreationTime)
              .AsQueryable();
        }

        public IQueryable<Branch> GetAllBranches()
        {
            return _dbContext.Branch
                .Include(b => b.Business)
                    .ThenInclude(b => b.Category)
                .Include(b => b.City)
                .OrderByDescending(b => b.CreationTime)
                .AsQueryable();
        }

        public int GetBranchCount()
        {
            return _dbContext.Branch
                .Where(b => b.IsActive)
                .Count();
        }
    }
}
