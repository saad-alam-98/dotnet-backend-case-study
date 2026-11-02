using SaudiPlus.Request;
using SaudiPlus.Response;
using SaudiPlus.Helper.Filter;
using SaudiPlus.Request.Partner;
using SaudiPlus.Response.Partner;

namespace SaudiPlus.Interfaces.Service
{
    public interface IPartnerService
    {
        Task<SuccessResponse> CreatePartner(CreatePartnerDTO model);
        Task<SuccessResponse> CreateBusinessPartner(Guid businessId);
        Task<SuccessResponse> EditPartner(EditPartnerDTO model);
        Task<SuccessResponse> TogglePartnerStatus(ToggleStatusDTO<int> model);
        Task<GetPartnerResponse> GetPartnerById(int id);
        Task<GetPartnersResponse> GetPartnersLocalized(EntityFilter filter);
        Task<GetPartnersResponse> GetPartnersDetailed(EntityFilter filter);
        Task<GetPartnersResponse> GetActivePartner();
        Task<GetBusinessForPartner> GetBusinessForPartner();
        Task<SuccessResponse> DeletePartner(int id);
    }
}
