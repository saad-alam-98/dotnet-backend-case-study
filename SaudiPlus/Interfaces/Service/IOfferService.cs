using SaudiPlus.Request;
using SaudiPlus.Response;
using SaudiPlus.Request.Offer;
using SaudiPlus.Helper.Filter;
using SaudiPlus.Response.Offer;
using SaudiPlus.Request.Category;
using SaudiPlus.Response.Category;
using SaudiPlus.Response.BusinessUser;

namespace SaudiPlus.Interfaces.Service
{
    public interface IOfferService
    {
        Task<GetOfferResponse> CreateOffer(Guid BusinessId, IOfferDTO model);
        Task<GetOfferResponse> EditOffer(EditOfferDTO model);
        Task<GetOfferResponse> GetOfferById(int offerId, LocationDTO? location);
        Task<SuccessResponse> ToggleOfferStatus(ToggleStatusDTO<int> model);
        Task<GetOffersResponse> GetTopOffers(LocationDTO? location);
        Task<GetOffersResponse> GetTopOfferByLocation(GetByLocationAndCategoryDTO model, Entities.IndividualUser user);
        Task<GetBusinessOffersResponse> GetBusinessOffers(Guid businessId, EntityFilter filter);
        Task<GetBusinessOffersResponse> GetBusinessActiveOffers(Entities.IndividualUser user, Guid businessId, LocationDTO? location);
        Task<GetOffersResponse> GetOffersByFilter(EntityFilter filter, LocationDTO? location);
        Task<OfferStatusResponse> GetOfferStatus(Guid businessId);
        Task<RedeemOfferResponse> RedeemOffer(Entities.IndividualUser user, int offerId);
        Task<UserRedeemOfferResponse> GetRedeemOffers(Entities.IndividualUser user);
        Task<GetFilterByTypeResponse> GetItemByCategory(TypeFilterDTO model);
        Task<RedeemOfferQrResponse> RedeemOfferByQrCode(Entities.IndividualUser user, RedeemOfferByQrDTO model);
        Task<GetBusinessForOffersResponse> GetBusinessForOffers();
        Task<GetSearchFilters> GetSearchFilter();
        Task<SuccessResponse> DeleteOffer(int offerId);
    }
}
