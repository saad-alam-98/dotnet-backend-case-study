using SaudiPlus.Entities;

namespace SaudiPlus.Interfaces.Repository
{
    public interface IBusinessRequestRepository : IRepository<BusinessRequest, int>
    {
        Task<BusinessRequest?> GetByBusinessName(string businessName);
        IQueryable<BusinessRequest> GetRequestForFilter();
        Task<BusinessRequest?> GetRequestWithRelation(int id);
    }
}
