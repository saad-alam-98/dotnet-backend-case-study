using SaudiPlus.Entities;
using SaudiPlus.Enums;

namespace SaudiPlus.Interfaces.Repository
{
    public interface IPolicyRepository : IRepository<Policy, int>
    {
        Task<Policy?> GetPolicyByType(PolicyType type);
    }
}
