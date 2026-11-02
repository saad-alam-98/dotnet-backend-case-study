using SaudiPlus.Entities;

namespace SaudiPlus.Interfaces.Repository
{
    public interface ISupportTypeRepository : IRepository<SupportType, int>
    {
        IQueryable<SupportType> GetSupportTypeForFilter();
    }
}
