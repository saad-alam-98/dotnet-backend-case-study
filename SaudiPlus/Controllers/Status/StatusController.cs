using SaudiPlus.Request;
using SaudiPlus.Response;
using SaudiPlus.Constants;
using SaudiPlus.Extensions;
using System.Security.Claims;
using SaudiPlus.Helper.Filter;
using SaudiPlus.Request.Status;
using Microsoft.AspNetCore.Mvc;
using SaudiPlus.Response.Status;
using SaudiPlus.Interfaces.Service;
using SaudiPlus.Interfaces.Repository;
using Swashbuckle.AspNetCore.Annotations;

namespace SaudiPlus.Controllers.Status
{
    [ApiController]
    [Route("api/[controller]")]
    public class StatusController : BaseController<StatusController>
    {
        private readonly IStatusService _service;
        private IIndividualUserRepository _individualUserRepository;
        public StatusController(
            IStatusService service,
            ILogger<StatusController> logger,
            IIndividualUserRepository individualUserRepository
        ) : base(logger)
        {
            _service = service;
            _individualUserRepository = individualUserRepository;
        }

        [HttpPost]
        [Route("[action]")]
        [AuthPermission(Permissions.Status.Create)]
        [SwaggerOperation(Tags = new[] { "Business", "Dashboard" })]
        public async Task<ActionResult<SuccessResponse>> CreateStatus([FromForm] CreateStatusDTO model)
        {
            try
            {
                var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return new SuccessResponse
                    (
                        StatusCodes.Status401Unauthorized,
                        "Unauthorized: Invalid User",
                        new SuccessResponsePayload { success = false }
                    );
                }

                if (!Guid.TryParse(userIdClaim.Value, out Guid userId))
                {
                    return new SuccessResponse
                    (
                        StatusCodes.Status401Unauthorized,
                        "Unauthorized: Invalid User",
                        new SuccessResponsePayload { success = false }
                    );
                }

                var response = await _service.CreateStatus(model, userId);
                return Ok(response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new SuccessResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: new SuccessResponsePayload { success = true }
                );
            }
        }

        [HttpPatch]
        [Route("[action]")]
        [AuthPermission(Permissions.Status.Edit)]
        [SwaggerOperation(Tags = new[] { "Business", "Dashboard" })]
        public async Task<ActionResult<SuccessResponse>> EditStatus([FromForm] EditStatusDTO model)
        {
            try
            {
                var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return new SuccessResponse
                    (
                        StatusCodes.Status401Unauthorized,
                        "Unauthorized: Invalid User",
                        new SuccessResponsePayload { success = false }
                    );
                }

                if (!Guid.TryParse(userIdClaim.Value, out Guid userId))
                {
                    return new SuccessResponse
                    (
                        StatusCodes.Status401Unauthorized,
                        "Unauthorized: Invalid User",
                        new SuccessResponsePayload { success = false }
                    );
                }

                var response = await _service.EditStatus(model, userId);
                return Ok(response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new SuccessResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: new SuccessResponsePayload { success = true }
                );
            }
        }

        [HttpGet]
        [Route("[action]")]
        [AuthPermission(Permissions.Status.View)]
        [SwaggerOperation(Tags = new[] { "Business", "Dashboard" })]
        public async Task<ActionResult<GetStatusDetailResponse>> GetStatusById([FromQuery] int id)
        {
            try
            {
                var response = await _service.GetStatusById(id);
                return Ok(response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetStatusDetailResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: null
                );
            }
        }

        [HttpGet]
        [Route("[action]")]
        [AuthPermission(Permissions.Status.View)]
        [SwaggerOperation(Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<GetStatusDetailResponse>> GetBusinessStatus([FromQuery] Guid businessId, [FromQuery] EntityFilter filter)
        {
            try
            {
                var response = await _service.GetBusinessStatus(businessId, filter);
                return Ok(response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetStatusDetailResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: null
                );
            }
        }

        [HttpGet]
        [Route("[action]")]
        [AuthPermission(Permissions.Status.View)]
        [SwaggerOperation(Tags = new[] { "Business", "Dashboard" })]
        public async Task<ActionResult<GetStatusDetailResponse>> GetStatusList([FromQuery] EntityFilter filter)
        {
            try
            {
                var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return new GetStatusDetailResponse
                    (
                        StatusCodes.Status401Unauthorized,
                        "Unauthorized: Invalid User",
                        null
                    );
                }

                if (!Guid.TryParse(userIdClaim.Value, out Guid userId))
                {
                    return new GetStatusDetailResponse
                    (
                        StatusCodes.Status401Unauthorized,
                        "Unauthorized: Invalid User",
                        null
                    );
                }

                var response = await _service.GetStatusList(userId, filter);
                return Ok(response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetStatusDetailResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: null
                );
            }
        }

        [HttpPatch]
        [Route("[action]")]
        [AuthPermission(Permissions.Status.Edit)]
        [SwaggerOperation(Tags = new[] { "Business", "Dashboard" })]
        public async Task<ActionResult<SuccessResponse>> ToggleStatus([FromQuery] ToggleStatusDTO<int> model)
        {
            try
            {
                var response = await _service.ToggleStatus(model);
                return Ok(response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new SuccessResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: new SuccessResponsePayload { success = true }
                );
            }
        }

        [HttpGet]
        [Route("[action]")]
        [AuthPermission(Permissions.Status.View)]
        [SwaggerOperation(Tags = new[] { "Mobile" })]
        public async Task<ActionResult<GetUserStatusListResponse>> GetActiveStatus()
        {
            try
            {
                var user = await HttpContext.GetUserFromContext(_individualUserRepository);
                if (user == null)
                {
                    return new GetUserStatusListResponse(
                        StatusCodes.Status401Unauthorized,
                        "Unauthorized: Invalid User",
                        null
                    );
                }
                var response = await _service.GetActiveStatus(user.Id);
                return Ok(response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetUserStatusListResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: null
                );
            }
        }

        [HttpGet]
        [Route("[action]")]
        [AuthPermission(Permissions.Status.View)]
        [SwaggerOperation(Tags = new[] { "Mobile" })]
        public async Task<ActionResult<SuccessResponse>> ViewStatus([FromQuery] int id)
        {
            try
            {
                var user = await HttpContext.GetUserFromContext(_individualUserRepository);
                if (user == null)
                {
                    return new SuccessResponse(
                        StatusCodes.Status401Unauthorized,
                        "Unauthorized: Invalid User",
                        new SuccessResponsePayload { success = false }
                    );
                }
                var response = await _service.ViewStatus(id, user.Id);
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
        [AuthPermission(Permissions.Status.View)]
        [SwaggerOperation(Tags = new[] { "Mobile" })]
        public async Task<ActionResult<SuccessResponse>> RecordClick([FromQuery] int id)
        {
            try
            {
                var user = await HttpContext.GetUserFromContext(_individualUserRepository);
                if (user == null)
                {
                    return new SuccessResponse(
                        StatusCodes.Status401Unauthorized,
                        "Unauthorized: Invalid User",
                        new SuccessResponsePayload { success = false }
                    );
                }
                var response = await _service.RecordClick(id, user.Id);
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
        [AuthPermission(Permissions.Status.View)]
        [SwaggerOperation(Tags = new[] { "Mobile" })]
        public async Task<ActionResult<SuccessResponse>> RecordInteractionTime([FromQuery] int id, [FromQuery] int seconds)
        {
            try
            {
                var user = await HttpContext.GetUserFromContext(_individualUserRepository);
                if (user == null)
                {
                    return new SuccessResponse(
                        StatusCodes.Status401Unauthorized,
                        "Unauthorized: Invalid User",
                        new SuccessResponsePayload { success = false }
                    );
                }
                var response = await _service.RecordInteractionTime(id, user.Id, seconds);
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

        [HttpDelete]
        [Route("[action]")]
        [AuthPermission(Permissions.Status.View)]
        [SwaggerOperation(Tags = new[] { "Business", "Dashboard" })]
        public async Task<ActionResult<SuccessResponse>> DeleteStatus([FromQuery] int id)
        {
            try
            {
                var response = await _service.DeleteStatus(id);
                return Ok(response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new SuccessResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: new SuccessResponsePayload { success = true }
                );
            }
        }
    }
}
