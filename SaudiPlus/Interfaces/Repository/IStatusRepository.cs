using SaudiPlus.Entities;

namespace SaudiPlus.Interfaces.Repository
{
    public interface IStatusRepository : IRepository<Status, int>
    {
        Task<Status?> GetStatusWithRelation(int id);
        IQueryable<Status> GetBusinessStatus(Guid businessId);
        IQueryable<Status> GetAdminStatus();
        IQueryable<Status> GetActiveStatus();
    }
}
