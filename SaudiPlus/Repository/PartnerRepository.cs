using Microsoft.EntityFrameworkCore;
using SaudiPlus.Entities;
using SaudiPlus.Interfaces.Repository;

namespace SaudiPlus.Repository
{
    public class PartnerRepository : Repository<Partner, int>, IPartnerRepository
    {
        public PartnerRepository(SaudiPlusDbContext dbContext) : base(dbContext)
        {
        }

        public IQueryable<Partner> GetPartnersForFilter()
        {
            return _dbContext.Partner
                .Include(b => b.Category)
                .OrderByDescending(b => b.CreationTime)
                .AsQueryable();
        }

        public async Task<List<Partner>> GetPartnersWithRelation()
        {
            return await _dbContext.Partner
                .Include(b => b.Category)
                .ToListAsync();
        }

        public async Task<Partner?> GetBusinessPartner(Guid businessId)
        {
            return await _dbContext.Partner
              .FirstOrDefaultAsync(p => p.BusinessUserId == businessId);
        }

        public async Task<Partner?> GetPartnerWithRelation(int id)
        {
            return await _dbContext.Partner
               .Include(p => p.Category)
               .FirstOrDefaultAsync(p => p.Id == id);
        }
    }
}