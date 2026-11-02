using SaudiPlus.Entities;
using Microsoft.EntityFrameworkCore;
using SaudiPlus.Interfaces.Repository;

namespace SaudiPlus.Repository
{
    public class UserOfferTokenRepository : Repository<UserOfferToken, int>, IUserOfferTokenRepository
    {
        public UserOfferTokenRepository(SaudiPlusDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<UserOfferToken?> GetByUserId(Guid userId)
        {
            return await _dbContext.UserOfferToken.
                FirstOrDefaultAsync(token => token.UserId == userId);
        }

        public async Task<UserOfferToken?> GetUserTokenWithRelation(string code)
        {
            return await _dbContext.UserOfferToken
                .Include(Token => Token.User)
                .FirstOrDefaultAsync(token => token.Code == code);
        }
    }
}
