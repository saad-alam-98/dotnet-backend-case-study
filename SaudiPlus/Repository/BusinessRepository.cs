using SaudiPlus.Entities;
using Microsoft.EntityFrameworkCore;
using SaudiPlus.Interfaces.Repository;

namespace SaudiPlus.Repository
{
    public class BusinessRepository : Repository<BusinessUser, Guid>, IBusinessRepository
    {
        public BusinessRepository(SaudiPlusDbContext dbContext) : base(dbContext)
        {

        }

        public async Task<List<BusinessUser>> GetBusinessWithCategory()
        {
            return await _dbContext.BusinessUsers
                .Include(b => b.Category)
                .ToListAsync();
        }

        public async Task<BusinessUser?> GetBusinessWithRelation(Guid businessId)
        {
            return await _dbContext.BusinessUsers
                .Include(b => b.Branches)
                .Include(b => b.Category)
                .Include(b => b.Offers)
                  .ThenInclude(o => o.Logos)
                .FirstOrDefaultAsync(b => b.Id == businessId);
        }

        public async Task<BusinessUser?> GetBusinessWithRelation(string Email)
        {
            return await _dbContext.BusinessUsers
                .Include(b => b.Category)
                .Include(b => b.Offers)
                .FirstOrDefaultAsync(b => b.Email == Email);
        }

        public IQueryable<BusinessUser> GetBusinessForFilter()
        {
            return _dbContext.BusinessUsers
               .Include(b => b.Branches)
               .Include(b => b.Category)
               .Include(b => b.Offers)
               .OrderByDescending(b => b.CreationTime)
               .AsQueryable();
        }

        public async Task<BusinessUser?> GetBusinessByUserId(Guid userId)
        {
            return await _dbContext.BusinessUsers
              .Include(b => b.Category)
              .Include(b => b.Offers)
              .FirstOrDefaultAsync(b => b.UserId == userId);
        }

        public async Task<List<BusinessUser>> GetBusinessForLocation()
        {
            return await _dbContext.BusinessUsers
                .Include(b => b.Category)
                .Include(b => b.Branches)
                .ToListAsync();
        }

        public async Task<List<BusinessUser>> GetBusinessWithPartner()
        {
            return await _dbContext.BusinessUsers
              .Include(b => b.Partner)
              .ToListAsync();
        }

        public async Task<List<BusinessUser>> GetBusinessByCateogry(int CategoryId)
        {
            return await _dbContext.BusinessUsers
               .Where(b => b.CategoryId == CategoryId)
               .Include(b => b.Category)
               .ToListAsync();
        }

        public int GetBusinessCount()
        {
            return _dbContext.BusinessUsers
                .Where(b => b.IsActive)
                .Count();
        }
    }
}
