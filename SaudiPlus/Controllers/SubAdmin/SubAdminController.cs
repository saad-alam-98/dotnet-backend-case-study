using SaudiPlus.Request;
using SaudiPlus.Response;
using SaudiPlus.Constants;
using SaudiPlus.Helper.Filter;
using Microsoft.AspNetCore.Mvc;
using SaudiPlus.Request.SubAdmin;
using SaudiPlus.Response.SubAdmin;
using SaudiPlus.Interfaces.Service;
using Swashbuckle.AspNetCore.Annotations;

namespace SaudiPlus.Controllers.SubAdmin
{
    [ApiController]
    [Route("api/[controller]")]
    public class SubAdminController : BaseController<SubAdminController>
    {
        private readonly ISubAdminService _service;
        public SubAdminController(
            ISubAdminService service,
            ILogger<SubAdminController> logger
        ) : base(logger)
        {
            _service = service;
        }

        [HttpGet]
        [Route("[action]")]
        [AuthPermission(Permissions.SubAdmin.Create, Permissions.SubAdmin.View)]
        [SwaggerOperation(Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<GetUserPermissionsResponse>> GetAdminPermissions()
        {
            try
            {
                var response = await _service.GetUserPermissions();
                return response;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetUserPermissionsResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    null
                );
            }
        }

        [HttpPost]
        [Route("[action]")]
        [AuthPermission(Permissions.SubAdmin.Create)]
        [SwaggerOperation(Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<SuccessResponse>> CreateSubAdminUser([FromBody] CreateSubAdminUserDTO model)
        {
            try
            {
                var response = await _service.CreateSubAdminUser(model);
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
        
        [HttpPatch]
        [Route("[action]")]
        [AuthPermission(Permissions.SubAdmin.Edit)]
        [SwaggerOperation(Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<SuccessResponse>> EditSubAdminUser([FromBody] EditSubAdminUserDTO model)
        {
            try
            {
                var response = await _service.EditSubAdminUser(model);
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

        [HttpGet]
        [Route("[action]")]
        [AuthPermission(Permissions.SubAdmin.View)]
        [SwaggerOperation(Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<GetSubAdminResponse>> GetSubAdminById([FromQuery] Guid Id)
        {
            try
            {
                var Response = await _service.GetSubAdminById(Id);
                return Ok(Response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetSubAdminResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: null
                );
            }
        }

        [HttpGet]
        [Route("[action]")]
        [AuthPermission(Permissions.SubAdmin.View)]
        [SwaggerOperation(Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<GetSubAdminsResponse>> GetSubAdmins([FromQuery] EntityFilter filter)
        {
            try
            {
                var Response = await _service.GetSubAdmins(filter);
                return Ok(Response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetSubAdminsResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: null
                );
            }
        }

        [HttpPatch]
        [Route("[action]")]
        [AuthPermission(Permissions.SubAdmin.Edit)]
        [SwaggerOperation(Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<SuccessResponse>> ToggleSubAdminStatus([FromBody] ToggleStatusDTO<Guid> model)
        {
            try
            {
                var response = await _service.ToggleSubAdminStatus(model);
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
        [AuthPermission(Permissions.SubAdmin.Delete)]
        [SwaggerOperation(Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<SuccessResponse>> DeleteSubAdmin([FromQuery] Guid Id)
        {
            try
            {
                var response = await _service.DeleteSubAdmin(Id);
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
