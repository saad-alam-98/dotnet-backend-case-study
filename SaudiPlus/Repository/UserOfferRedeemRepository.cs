using SaudiPlus.Entities;
using Microsoft.EntityFrameworkCore;
using SaudiPlus.Interfaces.Repository;

namespace SaudiPlus.Repository
{
    public class UserOfferRedeemRepository : Repository<UserOfferRedeem, int>, IUserOfferRedeemRepository
    {
        public UserOfferRedeemRepository(SaudiPlusDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<UserOfferRedeem?> GetByOfferAndUserId(Guid userId, int offerId)
        {
            return await _dbContext.UserOfferRedeem
                .OrderByDescending(x => x.CreationTime)
                .FirstOrDefaultAsync(redeem => redeem.OfferId == offerId && redeem.UserId == userId);
        }

        public async Task<List<Offer>> GetUserRedeemedOffers(Guid userId)
        {
            return await _dbContext.UserOfferRedeem
                .Where(redeem => redeem.UserId == userId)
                .Include(redeem => redeem.Offer)
                    .ThenInclude(offer => offer.Business)
                .Select(redeem => redeem.Offer)
                .ToListAsync();
        }
        public async Task<List<UserOfferRedeem>> GetRedeemsByYear(int year)
        {
            return await _dbContext.UserOfferRedeem
                .Where(redeem => redeem.CreationTime.Year == year)
                .ToListAsync();
        }
        public async Task<List<UserOfferRedeem>> GetRedeemsByMonth(int year, int month)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddSeconds(-1);
            
            return await _dbContext.UserOfferRedeem
                .Where(redeem => redeem.CreationTime >= startDate && redeem.CreationTime <= endDate)
                .ToListAsync();
        }
    }
}
