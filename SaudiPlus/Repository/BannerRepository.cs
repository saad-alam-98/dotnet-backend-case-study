using SaudiPlus.Entities;
using Microsoft.EntityFrameworkCore;
using SaudiPlus.Interfaces.Repository;

namespace SaudiPlus.Repository
{
    public class BannerRepository : Repository<Banner, int>, IBannerRepository
    {
        public BannerRepository(SaudiPlusDbContext dbContext) : base(dbContext)
        {
        }

        public IQueryable<Banner> GetBannersForFilter()
        {
            return _dbContext.Banner
                .Include(b => b.Business)
                .Include(b => b.Category)
                .Include(b => b.City)
                .AsQueryable();
        }

        public async Task<List<Banner>> GetBannersWithRelations()
        {
            return await _dbContext.Banner
                .Include(b => b.Business)
                .Include(b => b.Category)
                .Include(b => b.City)
                .ToListAsync();
        }

        public async Task<Banner?> GetBannerWithRelations(int id)
        {
            return await _dbContext.Banner
                .Include(b => b.Business)
                .Include(b => b.Category)
                .Include(b => b.City)
                .FirstOrDefaultAsync(b => b.Id == id);
        }
    }
}
