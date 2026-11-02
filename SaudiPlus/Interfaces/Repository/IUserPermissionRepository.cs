using SaudiPlus.Entities;

namespace SaudiPlus.Interfaces.Repository
{
    public interface IUserPermissionRepository : IRepository<UserPermission, int>
    {
        Task DeleteByUserIdAsync(Guid userId);
        Task<List<UserPermission>> GetByUserIdAsync(Guid userId);
    }
}
