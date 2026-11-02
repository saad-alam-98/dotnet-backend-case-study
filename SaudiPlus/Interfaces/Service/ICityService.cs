using SaudiPlus.Request;
using SaudiPlus.Response;
using SaudiPlus.Request.City;
using SaudiPlus.Response.City;
using SaudiPlus.Helper.Filter;
using SaudiPlus.Response.Banner;

namespace SaudiPlus.Interfaces.Service
{
    public interface ICityService
    {
        Task<SuccessResponse> AddCity(AddCityDTO model);
        Task<SuccessResponse> EditCity(EditCityDTO model);

        Task<GetCitiesResponse> GetActiveCity();
        Task<GetCitiesResponse> GetActiveCityLocalized();

        Task<GetCityResponse> GetCityById(int cityId);
        Task<GetCityResponse> GetCityByIdLocalized(int cityId);

        Task<GetCitiesResponse> GetCities();

        Task<GetCitiesResponse> GetCitiesByFilter(EntityFilter filter);
        Task<SuccessResponse> ToggleCityStatus(ToggleStatusDTO<int> model);
        Task<SuccessResponse> DeleteCity(int cityId);
    }
}
