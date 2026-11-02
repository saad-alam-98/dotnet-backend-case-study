using SaudiPlus.Entities;
using SaudiPlus.Interfaces.Repository;

namespace SaudiPlus.Repository
{
    public class SupportTypeRepository : Repository<SupportType, int>, ISupportTypeRepository
    {
        public SupportTypeRepository(SaudiPlusDbContext dbContext) : base(dbContext)
        {
        }

        public IQueryable<SupportType> GetSupportTypeForFilter()
        {
            return _dbContext.SupportType
                .AsQueryable();
        }
    }
}
