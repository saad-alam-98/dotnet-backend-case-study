using SaudiPlus.Request;
using SaudiPlus.Response;
using SaudiPlus.Constants;
using SaudiPlus.Request.City;
using SaudiPlus.Response.City;
using SaudiPlus.Helper.Filter;
using Microsoft.AspNetCore.Mvc;
using SaudiPlus.Interfaces.Service;
using Swashbuckle.AspNetCore.Annotations;

namespace SaudiPlus.Controllers.City
{
    [ApiController]
    [Route("api/[controller]")]
    public class CityController : BaseController<CityController>
    {
        private readonly ICityService _service;
        public CityController(
            ICityService cityService,
            ILogger<CityController> logger
        ) : base(logger)
        {
            this._service = cityService;
        }

        [HttpPost]
        [Route("[action]")]
        [AuthPermission(Permissions.City.Create)]
        [SwaggerOperation(Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<SuccessResponse>> AddCity([FromBody] AddCityDTO model)
        {
            try
            {
                var response = await _service.AddCity(model);
                return Ok(response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new SuccessResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: new SuccessResponsePayload { success = false }
                );
            }
        }

        [HttpPatch]
        [Route("[action]")]
        [AuthPermission(Permissions.City.Edit)]
        [SwaggerOperation(Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<SuccessResponse>> EditCity([FromBody] EditCityDTO model)
        {
            try
            {
                var response = await _service.EditCity(model);
                return Ok(response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new SuccessResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: new SuccessResponsePayload { success = false }
                );
            }
        }
        
        [HttpGet]
        [Route("[action]")]
        [AuthPermission(Permissions.City.View)]
        [SwaggerOperation(Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<GetCitiesResponse>> GetActiveCity()
        {
            try
            {
                var Response = await _service.GetActiveCity();
                return Ok(Response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetCitiesResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: null
                );
            }
        }

        [HttpGet]
        [Route("[action]")]
        public async Task<ActionResult<GetCitiesResponse>> GetActiveCityLocalized()
        {
            try
            {
                var Response = await _service.GetActiveCityLocalized();
                return Ok(Response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetCitiesResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: null
                );
            }
        }

        [HttpGet]
        [Route("[action]")]
        [AuthPermission(Permissions.City.View)]
        public async Task<ActionResult<GetCityResponse>> GetCityById([FromQuery] int cityId)
        {
            try
            {
                var Response = await _service.GetCityById(cityId);
                return Ok(Response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetCityResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: null
                );
            }
        }

        [HttpGet]
        [Route("[action]")]
        [AuthPermission(Permissions.City.View)]
        public async Task<ActionResult<GetCityResponse>> GetCityByIdLocalized([FromQuery] int cityId)
        {
            try
            {
                var Response = await _service.GetCityByIdLocalized(cityId);
                return Ok(Response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetCityResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: null
                );
            }
        }

        [HttpGet]
        [Route("[action]")]
        [AuthPermission(Permissions.City.View, Permissions.Banner.Create)]
        [SwaggerOperation(Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<GetCitiesResponse>> GetCities()
        {
            try
            {
                var Response = await _service.GetCities();
                return Ok(Response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetCitiesResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: null
                );
            }
        }

        [HttpGet]
        [Route("[action]")]
        [AuthPermission(Permissions.City.View)]
        [SwaggerOperation(Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<GetCitiesResponse>> GetCitiesByFilter([FromQuery] EntityFilter filter)
        {
            try
            {
                var business = await _service.GetCitiesByFilter(filter);
                return Ok(business);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetCitiesResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: null
                );
            }
        }

        [HttpPatch]
        [Route("[action]")]
        [AuthPermission(Permissions.City.Edit)]
        [SwaggerOperation(Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<SuccessResponse>> ToggleCityStatus([FromBody] ToggleStatusDTO<int> model)
        {
            try
            {
                var response = await _service.ToggleCityStatus(model);
                return response;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new SuccessResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    new SuccessResponsePayload { success = false }
                );
            }
        }


        [HttpDelete]
        [Route("[action]")]
        [AuthPermission(Permissions.City.Delete)]
        [SwaggerOperation(Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<SuccessResponse>> DeleteCity([FromQuery] int cityId)
        {
            try
            {
                var response = await _service.DeleteCity(cityId);
                return Ok(response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new SuccessResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: new SuccessResponsePayload { success = false }
                );
            }
        }
    }
}
