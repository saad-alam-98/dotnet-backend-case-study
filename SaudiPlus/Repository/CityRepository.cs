using SaudiPlus.Entities;
using Microsoft.EntityFrameworkCore;
using SaudiPlus.Interfaces.Repository;

namespace SaudiPlus.Repository
{
    public class CityRepository : Repository<City, int>, ICityRepository
    {
        public CityRepository(SaudiPlusDbContext dbContext) : base(dbContext)
        {
        }

        public IQueryable<City> GetCititesForFilter()
        {
            return _dbContext.City
            .AsQueryable();
        }

        public async Task<City?> GetCityWithBanners(int cityId)
        {
            return await _dbContext.City
                .Include(c => c.Banners)
                .FirstOrDefaultAsync(c => c.Id == cityId);
        }

        public async Task<bool> IsCityAssigned(int cityId)
        {
            return await _dbContext.IndividualUser.
                AnyAsync(user => user.CityId == cityId);
        }
    }
}
