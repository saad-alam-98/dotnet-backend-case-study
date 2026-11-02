using SaudiPlus.Request;
using SaudiPlus.Response;
using SaudiPlus.Request.Branch;
using SaudiPlus.Helper.Filter;
using SaudiPlus.Response.Branch;

namespace SaudiPlus.Interfaces.Service
{
    public interface IBranchService
    {
        Task<GetBranchResponse> CreateBranch(Guid BusinessId, ICreateBranchDTO model);
        Task<GetBranchResponse> EditBranch(EditBranchDTO model);
        Task<GetBranchResponse> GetBranchById(int branchId);
        Task<GetBranchQRCodeResponse> GetBranchQR(int branchId);
        Task<SuccessResponse> SetBranchQR(Guid BusinessId, int branchId);
        Task<SuccessResponse> ToggleBranchStatus(ToggleStatusDTO<int> model);
        Task<GetBranchesResponse> GetBusinessBranches(Guid businessId);
        Task<GetAllBranchResponse> GetAllBranches(EntityFilter filter);
        Task<GetBranchesPaginatedResponse> GetBranchByFilter(Guid businessId, EntityFilter filter);
        Task<GetBranchesResponse> GetBranch(Guid businessId);
        Task<GetBranchesResponse> GetBusinessActiveBranches(Guid businessId);
        Task<SuccessResponse> DeleteBranch(int branchId);
        Task<SuccessResponse> SyncBranchRating();
    }
}
