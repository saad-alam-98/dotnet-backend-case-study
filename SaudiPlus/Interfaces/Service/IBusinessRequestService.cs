using SaudiPlus.Request;
using SaudiPlus.Response;
using SaudiPlus.Helper.Filter;
using Microsoft.AspNetCore.Mvc;
using SaudiPlus.Request.BusinessRequest;
using SaudiPlus.Response.BusinessRequest;

namespace SaudiPlus.Interfaces.Service
{
    public interface IBusinessRequestService
    {
        Task<ActionResult<SuccessResponse>> CreateBusinessRequest(CreateBusinessRequestDTO model);
        Task<ActionResult<BusinessListRequestResponse>> GetBusinessRequestList(EntityFilter filter);
        Task<ActionResult<BusinessRequestResponse>> GetBusinessRequestById(int id);
        Task<ActionResult<SuccessResponse>> ToggleBusinessRequestStatus(ToggleStatusDTO<int> model);
        Task<ActionResult<SuccessResponse>> DeleteBusinessRequest(int id);
    }
}
