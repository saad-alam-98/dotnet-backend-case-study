using Microsoft.EntityFrameworkCore;
using SaudiPlus.Entities;
using SaudiPlus.Interfaces.Repository;

namespace SaudiPlus.Repository
{
    public class FavouriteRepository : Repository<Favourite, int>, IFavouriteRepository
    {
        public FavouriteRepository(SaudiPlusDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<Favourite?> GetFavourite(Guid userId, Guid businessUserId)
        {
            return await _dbContext
                .Favourite
                .FirstOrDefaultAsync(f => f.UserId == userId && f.BusinessUserId == businessUserId);
        }

        public async Task<List<BusinessUser>> GetFavouriteBusiness(Guid userId)
        {
            return await _dbContext
                .Favourite
                .Where(f => f.UserId == userId)
                .Select(f => f.Business)
                .ToListAsync();
        }

        public async Task<bool> IsFavourite(Guid userId, Guid businessUserId)
        {
            return await _dbContext
               .Favourite
               .AnyAsync(f => f.UserId == userId && f.BusinessUserId == businessUserId);
        }
    }
}