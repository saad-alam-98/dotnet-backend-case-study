using SaudiPlus.Entities;

namespace SaudiPlus.Interfaces.Repository
{
    public interface IContractRepository : IRepository<Contract, int>
    {
        Task<Contract?> GetBusinessContract(Guid businessId);
    }
}
