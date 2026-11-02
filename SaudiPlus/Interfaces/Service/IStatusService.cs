using SaudiPlus.Request;
using SaudiPlus.Response;
using SaudiPlus.Helper.Filter;
using SaudiPlus.Request.Status;
using SaudiPlus.Response.Status;

namespace SaudiPlus.Interfaces.Service
{
    public interface IStatusService
    {
        Task<SuccessResponse> CreateStatus(CreateStatusDTO model, Guid userId);
        Task<SuccessResponse> EditStatus(EditStatusDTO model, Guid userId);
        Task<GetStatusDetailResponse> GetStatusById(int id);
        Task<GetStatusListResponse> GetBusinessStatus(Guid businessId, EntityFilter filter);
        Task<GetStatusListResponse> GetStatusList(Guid userId, EntityFilter filter);
        Task<SuccessResponse> ToggleStatus(ToggleStatusDTO<int> model);
        Task<GetUserStatusListResponse> GetActiveStatus(Guid userId);
        Task<SuccessResponse> ViewStatus(int statusId, Guid userId);
        Task<SuccessResponse> RecordClick(int statusId, Guid userId);
        Task<SuccessResponse> RecordInteractionTime(int statusId, Guid userId, int seconds);
        Task<SuccessResponse> DeleteStatus(int id);
    }
}
