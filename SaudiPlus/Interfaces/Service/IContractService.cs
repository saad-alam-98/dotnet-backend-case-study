using SaudiPlus.Response;
using SaudiPlus.Request.Contract;
using SaudiPlus.Response.Contract;

namespace SaudiPlus.Interfaces.Service
{
    public interface IContractService
    {
        Task<SuccessResponse> AddContract(CreateContractDTO model);
        Task<SuccessResponse> EditContract(EditContractDTO model);

        Task<GetContractResponse> GetContract(Guid businessId);
        Task<SuccessResponse> DeleteContract(int ContractId);
    }
}
