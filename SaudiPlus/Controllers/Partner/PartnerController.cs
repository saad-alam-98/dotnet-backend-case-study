using SaudiPlus.Request;
using SaudiPlus.Response;
using SaudiPlus.Helper.Filter;
using Microsoft.AspNetCore.Mvc;
using SaudiPlus.Request.Partner;
using SaudiPlus.Response.Partner;
using SaudiPlus.Interfaces.Service;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.AspNetCore.Authorization;
using SaudiPlus.Constants;

namespace SaudiPlus.Controllers.Partner
{
    [ApiController]
    [Route("api/[controller]")]
    public class PartnerController : BaseController<PartnerController>
    {
        private IPartnerService _service;
        public PartnerController(
            IPartnerService service,
            ILogger<PartnerController> logger
        ) : base(logger)
        {
            _service = service;
        }

        [HttpPost]
        [Route("[action]")]
        [AuthPermission(Permissions.Partner.Create)]
        [SwaggerOperation(Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<SuccessResponse>> CreatePartner([FromForm] CreatePartnerDTO model)
        {
            try
            {
                var response = await _service.CreatePartner(model);
                return response;
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

        [HttpPost]
        [Route("[action]")]
        [AuthPermission(Permissions.Partner.Create)]
        [SwaggerOperation(Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<SuccessResponse>> CreateBusinessPartner([FromQuery] Guid businessId)
        {
            try
            {
                var response = await _service.CreateBusinessPartner(businessId);
                return response;
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
        [AuthPermission(Permissions.Partner.Edit)]
        [SwaggerOperation(Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<SuccessResponse>> EditPartner([FromForm] EditPartnerDTO model)
        {
            try
            {
                var response = await _service.EditPartner(model);
                return response;
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
        [AuthPermission(Permissions.Partner.Edit)]
        [SwaggerOperation(Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<SuccessResponse>> TogglePartnerStatus([FromBody] ToggleStatusDTO<int> model)
        {
            try
            {
                var response = await _service.TogglePartnerStatus(model);
                return response;
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
        [AuthPermission(Permissions.Partner.View)]
        [SwaggerOperation(Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<GetPartnerResponse>> GetPartnerById([FromQuery] int id)
        {
            try
            {
                var response = await _service.GetPartnerById(id);
                return response;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetPartnerResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    null
                );
            }
        }

        [HttpGet]
        [Route("[action]")]
        [AuthPermission(Permissions.Partner.View)]
        [SwaggerOperation(Tags = new[] { "Mobile" })]
        public async Task<ActionResult<GetPartnersResponse>> GetActivePartner()
        {
            try
            {
                var response = await _service.GetActivePartner();
                return response;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetPartnersResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    null
                );
            }
        }

        [HttpGet]
        [Route("[action]")]
        [AuthPermission(Permissions.Partner.View)]
        [SwaggerOperation(Tags = new[] { "Mobile", "Dashboard" })]
        public async Task<ActionResult<GetPartnersResponse>> GetPartnersLocalized([FromQuery] EntityFilter filter)
        {
            try
            {
                var response = await _service.GetPartnersLocalized(filter);
                return response;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetPartnersResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    null
                );
            }
        }

        [HttpGet]
        [Route("[action]")]
        [AuthPermission(Permissions.Partner.View)]
        [SwaggerOperation(Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<GetPartnersResponse>> GetPartnersDetailed([FromQuery] EntityFilter filter)
        {
            try
            {
                var response = await _service.GetPartnersDetailed(filter);
                return response;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetPartnersResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    null
                );
            }
        }

        [HttpGet]
        [Route("[action]")]
        [AuthPermission(Permissions.Partner.View)]
        [SwaggerOperation(Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<GetBusinessForPartner>> GetBusinessForPartner()
        {
            try
            {
                var response = await _service.GetBusinessForPartner();
                return response;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetBusinessForPartner
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    null
                );
            }
        }

        [HttpDelete]
        [Route("[action]")]
        [AuthPermission(Permissions.Partner.Delete)]
        [SwaggerOperation(Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<SuccessResponse>> DeletePartner([FromQuery] int id)
        {
            try
            {
                var response = await _service.DeletePartner(id);
                return response;
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
