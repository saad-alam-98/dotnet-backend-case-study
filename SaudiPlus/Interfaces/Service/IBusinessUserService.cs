using SaudiPlus.Request;
using SaudiPlus.Response;
using SaudiPlus.Entities;
using SaudiPlus.Request.Offer;
using SaudiPlus.Helper.Filter;
using SaudiPlus.Request.Category;
using SaudiPlus.Response.Category;
using SaudiPlus.Request.BusinessUser;
using SaudiPlus.Response.BusinessUser;

namespace SaudiPlus.Interfaces.Service
{
    public interface IBusinessUserService
    {
        Task<SuccessResponse> CreateBusinessUser(CreateBusinessDTO model);
        Task<SuccessResponse> AdminCreateBusinessUser(AdminCreateBusinessDTO model);

        Task<SuccessResponse> EditBusiness(Guid businessId, EditBusinessDTO model);
        Task<SuccessResponse> ToggleBusinessStatus(ToggleStatusDTO<Guid> model);

        Task<GetBusinessesResponse> GetBusiness();

        Task<GetBusinessResponse> GetBusinessByIdDetailed(Guid businessId);
        Task<GetBusinessResponse> GetBusinessByIdLocalized(Guid businessId);
        Task<GetBusinessDetailsById> GetBusinessDetailsById(Guid userId, Guid businessId, LocationDTO? location);
        Task<GetBusinessDetailsById> GetBusinessDetailsByBranch(Guid userId, int branchId, LocationDTO? location);

        Task<GetBusinessesResponse> GetActiveBusiness();
        Task<GetBusinessesResponse> GetActiveBusinessLocalized();

        Task<GetBusinessesResponse> GetTopBusinessLocalized();


        Task<GetBusinessesResponse> GetBusinessByFilter(EntityFilter filter);
        Task<GetBusinessesResponse> GetBusinessByFilterLocalized(EntityFilter paginationFilter);

        Task<SuccessResponse> SetBusinessContact(BusinessUser business, BusinessContactDTO model);
        Task<SuccessResponse> SetBusinessSocialMedia(BusinessUser business, SetSocialMediaDto model);
        Task<GetBusinessContactResponse> GetBusinessContact(Guid businessId);

        Task<BusinessStatsResponse> GetBusinessStats(Guid userId);
        Task<GetBusinessSalesResponse> GetMonthlySales(Guid businessUserId, int? year);
        Task<GetOfferRedeemStateResponse> GetMonthlyOfferRedeemCount(Guid businessUserId, int? year);

        Task<GetBusinessByLocationResponse> GetBusinessByLocation(IndividualUser user, GetByLocationAndCategoryDTO model);

        Task<SuccessResponse> SetBusinessAppLinks(BusinessUser business, SetBusinessAppLinksDTO model);
        Task<SuccessResponse> SetBusinessWebiste(BusinessUser business, SetWebsiteLinkDTO model);

        Task<GetBusinessAppResponse> GetBusinessAppLinks(Guid businessId);
        Task<GetBusinessWebisteResponse> GetBusinessWebiste(Guid businessId);

        Task<GetFilterByTypeResponse> GetItemByCategory(TypeFilterDTO model);

        Task<SuccessResponse> DeleteBusiness(Guid businessId);
    }
}
