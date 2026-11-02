using SaudiPlus.Request;
using SaudiPlus.Response;
using SaudiPlus.Constants;
using SaudiPlus.Helper.Filter;
using Microsoft.AspNetCore.Mvc;
using SaudiPlus.Interfaces.Service;
using SaudiPlus.Request.BusinessRequest;
using SaudiPlus.Response.BusinessRequest;
using Swashbuckle.AspNetCore.Annotations;

namespace SaudiPlus.Controllers.BusinessRequest
{
    [ApiController]
    [Route("api/[controller]")]
    public class BusinessRequestController : BaseController<BusinessRequestController>
    {
        private IBusinessRequestService _service;
        public BusinessRequestController(
            IBusinessRequestService service,
            ILogger<BusinessRequestController> logger
        ) : base(logger)
        {
            _service = service;
        }

        [HttpPost]
        [Route("[action]")]
        [SwaggerOperation(Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<SuccessResponse>> CreateBusinessRequest([FromBody] CreateBusinessRequestDTO model)
        {
            try
            {
                var response = await _service.CreateBusinessRequest(model);
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
        [AuthPermission(Permissions.BusinessRequest.Edit)]
        [SwaggerOperation(Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<SuccessResponse>> ToggleBusinessRequestStatus([FromBody] ToggleStatusDTO<int> model)
        {
            try
            {
                var response = await _service.ToggleBusinessRequestStatus(model);
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
        [AuthPermission(Permissions.BusinessRequest.View)]
        [SwaggerOperation(Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<BusinessRequestResponse>> GetBusinessRequestById([FromQuery] int id)
        {
            try
            {
                var response = await _service.GetBusinessRequestById(id);
                return response;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new BusinessRequestResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    null
                );
            }
        }

        [HttpGet]
        [Route("[action]")]
        [AuthPermission(Permissions.BusinessRequest.View)]
        [SwaggerOperation(Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<BusinessListRequestResponse>> GetBusinessRequestList([FromQuery] EntityFilter filter)
        {
            try
            {
                var response = await _service.GetBusinessRequestList(filter);
                return response;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new BusinessListRequestResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    null
                );
            }
        }


        [HttpDelete]
        [Route("[action]")]
        [AuthPermission(Permissions.BusinessRequest.Delete)]
        [SwaggerOperation(Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<SuccessResponse>> DeleteBusinessRequest([FromQuery] int id)
        {
            try
            {
                var response = await _service.DeleteBusinessRequest(id);
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