using SaudiPlus.Entities;

namespace SaudiPlus.Interfaces.Repository
{
    public interface IBranchRepository : IRepository<Branch, int>
    {
        IQueryable<Branch> GetAllBranches();
        IQueryable<Branch> GetBranchForFilter(Guid businessId);
        Task<List<Branch>> GetBranchWithRelations();
        Task<Branch?> GetBranchWithRelations(int id);
        Task<Branch?> GetBranchByBranchNumber(int BranchNumber);
        Task<List<Branch>> GetBusinessBranches(Guid businessId);
        int GetBranchCount();
    }
}
