using SaudiPlus.Request;
using SaudiPlus.Response;
using SaudiPlus.Helper.Filter;
using SaudiPlus.Request.SubAdmin;
using SaudiPlus.Response.SubAdmin;

namespace SaudiPlus.Interfaces.Service
{
    public interface ISubAdminService
    {
        Task<GetUserPermissionsResponse> GetUserPermissions();
        Task<SuccessResponse> CreateSubAdminUser(CreateSubAdminUserDTO model);
        Task<SuccessResponse> EditSubAdminUser(EditSubAdminUserDTO model);
        Task<GetSubAdminResponse> GetSubAdminById(Guid Id);
        Task<GetSubAdminsResponse> GetSubAdmins(EntityFilter filter);
        Task<SuccessResponse> ToggleSubAdminStatus(ToggleStatusDTO<Guid> model);
        Task<SuccessResponse> DeleteSubAdmin(Guid Id);
    }
}
