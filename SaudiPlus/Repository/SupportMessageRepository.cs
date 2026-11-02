using SaudiPlus.Entities;
using SaudiPlus.Interfaces.Repository;

namespace SaudiPlus.Repository
{
    public class SupportMessageRepository : Repository<SupportMessage, int>, ISupportMessageRepository
    {
        public SupportMessageRepository(SaudiPlusDbContext dbContext) : base(dbContext)
        {
        }
    }
}
