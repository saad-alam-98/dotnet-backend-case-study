using SaudiPlus.Entities;
using SaudiPlus.Enums;

namespace SaudiPlus.Interfaces.Repository
{
    public interface IBusinessPolicyRepository : IRepository<BusinessPolicy, int>
    {
        Task<BusinessPolicy?> GetBusinessPolicyByType(Guid BusinessId, PolicyType type);
    }
}
