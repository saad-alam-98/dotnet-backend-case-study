using SaudiPlus.Entities;

namespace SaudiPlus.Interfaces.Repository
{
    public interface IBusinessRepository : IRepository<BusinessUser, Guid>
    {
        Task<List<BusinessUser>> GetBusinessWithCategory();
        IQueryable<BusinessUser> GetBusinessForFilter();
        Task<BusinessUser?> GetBusinessWithRelation(Guid businessId);
        Task<BusinessUser?> GetBusinessByUserId(Guid userId);
        Task<BusinessUser?> GetBusinessWithRelation(string Email);
        Task<List<BusinessUser>> GetBusinessForLocation();
        Task<List<BusinessUser>> GetBusinessWithPartner();
        Task<List<BusinessUser>> GetBusinessByCateogry(int CategoryId);
        int GetBusinessCount();
    }
}
