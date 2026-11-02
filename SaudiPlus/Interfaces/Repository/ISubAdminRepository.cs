using SaudiPlus.Entities;

namespace SaudiPlus.Interfaces.Repository
{
    public interface ISubAdminRepository : IRepository<SubAdminUser, Guid>
    {
        IQueryable<SubAdminUser> GetSubAdminsForFilter();
        Task<SubAdminUser?> GetSubAdminUserByRelation(Guid id);
        Task<SubAdminUser?> GetSubAdminUserByUserId(Guid id);
        Task<SubAdminUser?> GetSubAdminUserByEmail(string email);
    }
}
