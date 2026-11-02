using SaudiPlus.Response;
using SaudiPlus.Extensions;
using SaudiPlus.Helper.Filter;
using Microsoft.AspNetCore.Mvc;
using SaudiPlus.Interfaces.Service;
using SaudiPlus.Request.Notification;
using SaudiPlus.Interfaces.Repository;
using SaudiPlus.Response.Notification;
using Microsoft.AspNetCore.Authorization;
using Swashbuckle.AspNetCore.Annotations;
using SaudiPlus.Constants;

namespace SaudiPlus.Controllers.Notification
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : BaseController<NotificationController>
    {
        private readonly INotificationCenterService _service;
        private readonly IIndividualUserRepository _individualUserRepository;


        public NotificationController(
            INotificationCenterService service,
            IIndividualUserRepository individualUserRepository,
            ILogger<NotificationController> logger
        ) : base(logger)
        {
            _service = service;
            _individualUserRepository = individualUserRepository;
        }

        [HttpGet]
        [Route("[action]")]
        [AuthPermission(Permissions.Notification.Create)]
        [SwaggerOperation(Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<NotificationUserResponse>> GetUsersForNotification([FromQuery] EntityBySearchFilter filter)
        {
            try
            {
                var response = await _service.GetUsersForNotification(filter);
                return response;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new NotificationUserResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: null
                );
            }
        }

        [HttpPost]
        [Route("[action]")]
        [AuthPermission(Permissions.Notification.Create)]
        [SwaggerOperation(Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<SuccessResponse>> SendGeneralNotification([FromBody] GeneralNotificationRequestDTO model)
        {
            try
            {
                var response = await _service.SendGeneralNotification(model);
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
        [AuthPermission(Permissions.Notification.Create)]
        [SwaggerOperation(Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<SuccessResponse>> SendIndividualNotification([FromBody] IndividualNotificationRequestDTO model)
        {
            try
            {
                var response = await _service.SendIndividualNotification(model);
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
        [AuthPermission(Permissions.Notification.Create)]
        [SwaggerOperation(Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<SuccessResponse>> SendGroupNotification([FromBody] GroupUsersNotificationRequestDTO model)
        {
            try
            {
                var response = await _service.SendGroupNotification(model);
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
        [AuthPermission(Permissions.Notification.View)]
        [SwaggerOperation(Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<GetNotificationResponse>> GetGroupNotifications([FromQuery] EntityFilter filter)
        {
            try
            {
                var response = await _service.GetGroupNotifications(filter);
                return response;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetNotificationResponse(
                    StatusCodes.Status500InternalServerError,
                    ex.Message,
                    null
                );
            }
        }

        [HttpGet]
        [Route("[action]")]
        [AuthPermission(Permissions.Notification.View)]
        [SwaggerOperation(Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<GetNotificationResponse>> GetGeneralNotifications([FromQuery] EntityFilter filter)
        {
            try
            {
                var response = await _service.GetGeneralNotifications(filter);
                return response;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetNotificationResponse(
                    StatusCodes.Status500InternalServerError,
                    ex.Message,
                    null
                );
            }
        }

        [HttpGet]
        [Route("[action]")]
        [AuthPermission(Permissions.Notification.View)]
        [SwaggerOperation(Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<GetNotificationResponse>> GetIndividualNotifications([FromQuery] EntityFilter filter)
        {
            try
            {
                var response = await _service.GetIndividualNotifications(filter);
                return response;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetNotificationResponse(
                    StatusCodes.Status500InternalServerError,
                    ex.Message,
                    null
                );
            }
        }

        [HttpPost]
        [Route("[action]")]
        [AuthPermission(Permissions.Notification.Create)]
        [SwaggerOperation(Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<SuccessResponse>> SendIndividualNotificationById([FromQuery] int id)
        {
            try
            {
                var response = await _service.SendIndividualNotificationById(id);
                return response;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new SuccessResponse(
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: new SuccessResponsePayload { success = false }
                );
            }
        }

        [HttpPost]
        [Route("[action]")]
        [AuthPermission(Permissions.Notification.Create)]
        [SwaggerOperation(Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<SuccessResponse>> SendGroupNotificationById([FromQuery] int id)
        {
            try
            {
                var response = await _service.SendGroupNotificationById(id);
                return response;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new SuccessResponse(
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: new SuccessResponsePayload { success = false }
                );
            }
        }

        [HttpPost]
        [Route("[action]")]
        [AuthPermission(Permissions.Notification.Create)]
        [SwaggerOperation(Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<SuccessResponse>> SendGeneralNotificationById([FromQuery] int id)
        {
            try
            {
                var response = await _service.SendGeneralNotificationById(id);
                return response;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new SuccessResponse(
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: new SuccessResponsePayload { success = false }
                );
            }
        }

        [HttpGet]
        [Route("[action]")]
        [AuthPermission(Permissions.Notification.View)]
        [SwaggerOperation(Tags = new[] { "Mobile" })]
        public async Task<ActionResult<GetSingleNotificationResponse>> MyNotifications([FromQuery] EntityFilter filter)
        {
            try
            {
                var user = await HttpContext.GetUserFromContext(_individualUserRepository);
                if (user == null)
                {
                    return new GetSingleNotificationResponse(
                        StatusCodes.Status401Unauthorized,
                        "Unauthorized: Invalid User",
                        null
                    );
                }

                var response = await _service.MyNotifications(user, filter);
                return Ok(response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetSingleNotificationResponse(
                    StatusCodes.Status500InternalServerError,
                    "An unexpected error occurred. Please try again later.",
                    null
                );
            }
        }
    }
}
