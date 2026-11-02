using SaudiPlus.Request;
using SaudiPlus.Response;
using SaudiPlus.Constants;
using SaudiPlus.Helper.Filter;
using SaudiPlus.Request.Banner;
using Microsoft.AspNetCore.Mvc;
using SaudiPlus.Response.Banner;
using SaudiPlus.Interfaces.Service;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.AspNetCore.Authorization;

namespace SaudiPlus.Controllers.Banners
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class BannersController : BaseController<BannersController>
    {
        private readonly IBannerService _service;
        public BannersController(
            ILogger<BannersController> logger,
            IBannerService bannerService
        ) : base(logger)
        {
            _service = bannerService;
        }

        [HttpPost]
        [Route("[action]")]
        [AuthPermission(Permissions.Banner.Create)]
        [SwaggerOperation(Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<GetBannerResponse>> CreateBanner([FromForm] CreateBannerDTO model)
        {
            try
            {
                var response = await _service.CreateBanner(model);
                return response;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetBannerResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    null
                );
            }
        }

        [HttpGet]
        [Route("[action]")]
        [AuthPermission(Permissions.Banner.View)]
        [SwaggerOperation(Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<GetBannersResponse>> GetAllBanners()
        {
            try
            {
                var response = await _service.GetAllBanner();
                return response;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetBannersResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: null
                );
            }
        }

        [HttpGet]
        [Route("[action]")]
        [AuthPermission(Permissions.Banner.View)]
        public async Task<ActionResult<GetBannerResponse>> GetBannerById([FromQuery] int id)
        {
            try
            {
                var response = await _service.GetBannerById(id);
                return response;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetBannerResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: null
                );
            }
        }

        [HttpGet]
        [Route("[action]")]
        [AuthPermission(Permissions.Banner.View)]
        [SwaggerOperation(Tags = new[] { "Mobile" })]
        public async Task<ActionResult<GetBannersResponse>> GetActiveBanner()
        {
            try
            {
                var response = await _service.GetActiveBanner();
                return response;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetBannersResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: null
                );
            }
        }

        [HttpPatch]
        [Route("[action]")]
        [AuthPermission(Permissions.Banner.Edit)]
        [SwaggerOperation(Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<GetBannerResponse>> EditBanner([FromForm] EditBannerDTO model)
        {
            try
            {
                var response = await _service.EditBanner(model);
                return response;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetBannerResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                   null
                );
            }
        }

        [HttpGet]
        [Route("[action]")]
        [AuthPermission(Permissions.Banner.View)]
        [SwaggerOperation(Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<GetBannersResponse>> GetBannerByFilter([FromQuery] EntityFilter filter)
        {
            try
            {
                var business = await _service.GetBannerByFilter(filter);
                return Ok(business);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetBannersResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: null
                );
            }
        }

        [HttpPatch]
        [Route("[action]")]
        [AuthPermission(Permissions.Banner.Edit)]
        [SwaggerOperation(Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<SuccessResponse>> ToggleBannerStatus([FromBody] ToggleStatusDTO<int> model)
        {
            try
            {
                var response = await _service.ToggleBannerStatus(model);
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
        [AuthPermission(Permissions.Banner.Delete)]
        [SwaggerOperation(Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<SuccessResponse>> DeleteBanner([FromQuery] int id)
        {
            try
            {
                var response = await _service.DeleteBanner(id);
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
    }
}
