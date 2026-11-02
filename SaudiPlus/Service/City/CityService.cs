using AutoMapper;
using SaudiPlus.Request;
using SaudiPlus.Response;
using SaudiPlus.Interfaces;
using SaudiPlus.Request.City;
using SaudiPlus.Response.City;
using SaudiPlus.Helper.Filter;
using SaudiPlus.Interfaces.Service;
using Microsoft.EntityFrameworkCore;
using SaudiPlus.Interfaces.Repository;
using SaudiPlus.Repository;

namespace SaudiPlus.Service.City
{
    public class CityService : BaseService<CityService>, ICityService
    {
        private readonly ICityRepository _cityRepository;
        public CityService(
            ICityRepository cityRepository,
            ILanguageService languageService,
            ILogger<CityService> logger,
            IMapper mapper
        ) : base(mapper, logger, languageService)
        {
            _cityRepository = cityRepository;
        }

        public async Task<SuccessResponse> AddCity(AddCityDTO model)
        {
            try
            {
                var city = _mapper.Map<Entities.City>(model);
                await _cityRepository.InsertAsync(city, true);

                return new SuccessResponse
                (
                    statusCode: StatusCodes.Status200OK,
                    message: "City Created",
                    payload: new SuccessResponsePayload { success = true }
                );
            }
            catch (Exception ex)
            {
                // Check if it's a unique constraint violation
                if (ex.InnerException != null && 
                   (ex.InnerException.Message.Contains("IX_City_NameEn") || 
                    ex.InnerException.Message.Contains("IX_City_NameAr")))
                {
                    return new SuccessResponse
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: "A city with this name already exists",
                        payload: new SuccessResponsePayload { success = false }
                    );
                }

                return new SuccessResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: new SuccessResponsePayload { success = false }
                );
            }
        }
        
        public async Task<SuccessResponse> EditCity(EditCityDTO model)
        {
            var existingCity = await _cityRepository.GetByIdAsync(model.Id);
            if (existingCity == null)
            {
                return new SuccessResponse(
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "City not found",
                    payload: new SuccessResponsePayload { success = false }
                );
            }

            try
            {
                model.IsActive = model.IsActive.HasValue ? model.IsActive.Value : existingCity.IsActive;
                _mapper.Map(model, existingCity);

                await _cityRepository.UpdateAsync(existingCity, true);

                return new SuccessResponse(
                    statusCode: StatusCodes.Status200OK,
                    message: "City Updated",
                    payload: new SuccessResponsePayload { success = true }
                );
            }
            catch
            {
                return new SuccessResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: new SuccessResponsePayload { success = false }
                );
            }
        }

        public async Task<GetCitiesResponse> GetActiveCity()
        {
            try
            {
                var response = (await _cityRepository.GetAsync())
                     .Where(r => r.IsActive);

                var payload = new GetCityPayload()
                {
                    Items = [],
                    TotalItems = response.Count(),
                    HasNextPage = false
                };
                foreach (var r in response)
                {
                    var item = _mapper.Map<CityInfoDetailed>(r);
                    payload.Items.Add(item);
                }

                return new GetCitiesResponse
                (
                    statusCode: StatusCodes.Status200OK,
                    message: "Cities",
                    payload: payload
                );
            }
            catch
            {
                return new GetCitiesResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: null
                );
            }
        }

        public async Task<GetCitiesResponse> GetActiveCityLocalized()
        {
            try
            {
                var (languageCode, error) = _languageService.GetLanguageCode();
                if (error != null)
                {
                    return new GetCitiesResponse
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: error.Message,
                        payload: null
                    );
                }

                var response = (await _cityRepository.GetAsync())
                     .Where(r => r.IsActive);

                var payload = new GetCityPayload()
                {
                    Items = [],
                    TotalItems = response.Count(),
                    HasNextPage = false
                };
                foreach (var r in response)
                {
                    var item = _mapper.Map<CityInfoLocalized>(r);
                    item.Name = r.GetNameByLanguageCode(languageCode);
                    payload.Items.Add(item);
                }

                return new GetCitiesResponse
                (
                    statusCode: StatusCodes.Status200OK,
                    message: "Cities",
                    payload: payload
                );
            }
            catch (Exception ex)
            {
                return new GetCitiesResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: null
                );
            }
        }

        public async Task<GetCityResponse> GetCityById(int cityId)
        {
            try
            {
                var response = await _cityRepository.GetCityWithBanners(cityId);
                if (response == null)
                {
                    return new GetCityResponse
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: $"City with Id {cityId} not found",
                        payload: null
                    );
                }
                var item = _mapper.Map<CityInfoDetailed>(response);

                return new GetCityResponse
                (
                    statusCode: StatusCodes.Status200OK,
                    message: "Cities",
                    payload: item
                );
            }
            catch (Exception ex)
            {
                return new GetCityResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: null
                );
            }
        }

        public async Task<GetCityResponse> GetCityByIdLocalized(int cityId)
        {
            try
            {
                var (languageCode, error) = _languageService.GetLanguageCode();
                if (error != null)
                {
                    return new GetCityResponse
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: error.Message,
                        payload: null
                    );
                }

                var response = await _cityRepository.GetCityWithBanners(cityId);
                if (response == null)
                {
                    return new GetCityResponse
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: $"City with Id {cityId} not found",
                        payload: null
                    );
                }
                var item = _mapper.Map<CityInfoLocalized>(response);
                item.Name = response.GetNameByLanguageCode(languageCode);

                return new GetCityResponse
                (
                    statusCode: StatusCodes.Status200OK,
                    message: "Cities",
                    payload: item
                );
            }
            catch (Exception ex)
            {
                return new GetCityResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: null
                );
            }
        }

        public async Task<GetCitiesResponse> GetCities()
        {
            try
            {
                var response = await _cityRepository.GetAsync();

                var payload = new GetCityPayload()
                {
                    Items = [],
                    TotalItems = response.Count(),
                    HasNextPage = false
                };
                foreach (var r in response)
                {
                    var item = _mapper.Map<CityInfoDetailed>(r);
                    payload.Items.Add(item);
                }

                return new GetCitiesResponse
                (
                    statusCode: StatusCodes.Status200OK,
                    message: "Cities",
                    payload: payload
                );
            }
            catch (Exception ex)
            {
                return new GetCitiesResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: null
                );
            }
        }

        public async Task<SuccessResponse> ToggleCityStatus(ToggleStatusDTO<int> model)
        {
            try
            {
                var response = await _cityRepository.GetByIdAsync(model.Id);
                if (response == null)
                {
                    return new SuccessResponse
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: $"City with Id {model.Id} not found",
                        payload: new SuccessResponsePayload { success = false }
                    );
                }

                response.IsActive = model.IsActive;
                await _cityRepository.UpdateAsync(response, true);

                return new SuccessResponse
                (
                    statusCode: StatusCodes.Status200OK,
                    message: "City",
                    payload: new SuccessResponsePayload { success = true }
                );
            }
            catch (Exception ex)
            {
                return new SuccessResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: new SuccessResponsePayload { success = false }
                );
            }
        }

        public async Task<SuccessResponse> DeleteCity(int cityId)
        {
            try
            {
                var isCityIdAssigned = await _cityRepository.IsCityAssigned(cityId);
                if (isCityIdAssigned)
                {
                    return new SuccessResponse
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: "This city cannot be deleted because it is in use.",
                        payload: new SuccessResponsePayload { success = false }
                    );
                }

                await _cityRepository.DeleteAsync(cityId, true);
                return new SuccessResponse
                (
                    statusCode: StatusCodes.Status200OK,
                    message: "City Deleted",
                    payload: new SuccessResponsePayload { success = true }
                );
            }
            catch (Exception ex)
            {
                return new SuccessResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: new SuccessResponsePayload { success = false }
                );
            }
        }

        public async Task<GetCitiesResponse> GetCitiesByFilter(EntityFilter filter)
        {
            try
            {
                var (languageCode, languageError) = _languageService.GetLanguageCode();
                if (languageError != null)
                {
                    return new GetCitiesResponse
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: languageError.Message,
                        payload: null
                    );
                }

                var query = _cityRepository.GetCititesForFilter();
                if (filter.isActive.HasValue)
                {
                    query = query
                        .Where(City => City.IsActive == filter.isActive.Value);
                }

                if (!string.IsNullOrEmpty(filter.searchKeyword))
                {
                    var searchKeyword = filter.searchKeyword.ToLower();
                    query = query.Where(banner =>
                        banner.NameEn.ToLower().Contains(searchKeyword) ||
                        banner.NameAr.ToLower().Contains(searchKeyword));
                }

                var totalItems = await query.CountAsync();
                var cities = await query
                    .Skip((filter.pageNumber - 1) * filter.pageSize)
                    .Take(filter.pageSize)
                    .ToListAsync();

                bool hasNextPage = (filter.pageNumber * filter.pageSize) < totalItems;

                var payload = new GetCityPayload
                {
                    Items = [],
                    TotalItems = totalItems,
                    HasNextPage = hasNextPage
                };

                foreach (var city in cities)
                {
                    var item = _mapper.Map<CityInfoDetailed>(city);
                    payload.Items.Add(item);
                }

                return new GetCitiesResponse
                (
                    statusCode: StatusCodes.Status200OK,
                    message: "Users Fetch Success",
                    payload: payload
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching user");
                return new GetCitiesResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: null
                );
            }
        }
    }
}
