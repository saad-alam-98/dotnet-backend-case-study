using SaudiPlus.Entities;
using SaudiPlus.Interfaces.Repository;

namespace SaudiPlus.Repository
{
    public class OfferLogoRepository : Repository<OfferLogo, int>, IOfferLogoRepository
    {
        public OfferLogoRepository(SaudiPlusDbContext dbContext) : base(dbContext)
        {
        }
    }
}
