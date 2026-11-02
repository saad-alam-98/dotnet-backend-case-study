using SaudiPlus.Request;
using SaudiPlus.Response;
using SaudiPlus.Helper.Filter;
using SaudiPlus.Request.Banner;
using SaudiPlus.Response.Banner;

namespace SaudiPlus.Interfaces.Service
{
    public interface IBannerService
    {
        Task<GetBannerResponse> CreateBanner(CreateBannerDTO model);
        Task<GetBannerResponse> GetBannerById(int id);
        Task<GetBannersResponse> GetAllBanner();
        Task<GetBannersResponse> GetActiveBanner();
        Task<GetBannerResponse> EditBanner(EditBannerDTO model);
        Task<SuccessResponse> ToggleBannerStatus(ToggleStatusDTO<int> model);
        Task<GetBannersResponse> GetBannerByFilter(EntityFilter filter);
        Task<SuccessResponse> DeleteBanner(int id);
    }
}
