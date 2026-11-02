using SaudiPlus.Request;
using SaudiPlus.Response;
using SaudiPlus.Constants;
using SaudiPlus.Extensions;
using SaudiPlus.Helper.Filter;
using Microsoft.AspNetCore.Mvc;
using SaudiPlus.Request.Support;
using SaudiPlus.Response.Support;
using SaudiPlus.Interfaces.Service;
using SaudiPlus.Interfaces.Repository;
using Swashbuckle.AspNetCore.Annotations;

namespace SaudiPlus.Controllers.Support
{
    [ApiController]
    [Route("api/[controller]")]
    public class SupportController : BaseController<SupportController>
    {
        private ISupportService _service;
        private readonly IIndividualUserRepository _individualUserRepository;
        public SupportController(
            ISupportService service,
            IIndividualUserRepository individualUserRepository,
            ILogger<SupportController> logger
        ) : base(logger)
        {
            _service = service;
            _individualUserRepository = individualUserRepository;
        }

        [HttpPost]
        [Route("[action]")]
        [AuthPermission(Permissions.Support.Create)]
        [SwaggerOperation(Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<SuccessResponse>> CreateSupportType([FromBody] AddSupportTypeDTO model)
        {
            try
            {
                var response = await _service.CreateSupportType(model);
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
        [AuthPermission(Permissions.Support.Edit)]
        [SwaggerOperation(Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<SuccessResponse>> EditSupportType([FromBody] EditSupportTypeDTO model)
        {
            try
            {
                var response = await _service.EditSupportType(model);
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
        [AuthPermission(Permissions.Support.Edit)]
        [SwaggerOperation(Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<SuccessResponse>> ToggleSupportType([FromBody] ToggleStatusDTO<int> model)
        {
            try
            {
                var response = await _service.ToggleSupportType(model);
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
        [AuthPermission(Permissions.Support.View)]
        [SwaggerOperation(Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<GetSupportTypes>> GetSupportTypes([FromQuery] EntityFilter filter)
        {
            try
            {
                var response = await _service.GetSupportTypes(filter);
                return response;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetSupportTypes
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    null
                );
            }
        }

        [HttpGet]
        [Route("[action]")]
        [AuthPermission(Permissions.Support.View)]
        [SwaggerOperation(Tags = new[] { "Mobile" })]
        public async Task<ActionResult<GetSupportTypes>> GetActiveSupportTypes()
        {
            try
            {
                var response = await _service.GetActiveSupportTypes();
                return response;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetSupportTypes
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    null
                );
            }
        }

        [HttpPost]
        [Route("[action]")]
        [AuthPermission(Permissions.Support.Create)]
        [SwaggerOperation(Tags = new[] { "Mobile" })]
        public async Task<ActionResult<CreateTicketResponse>> CreateSupportTicket([FromBody] CreateSupportTicketDTO model)
        {
            try
            {
                var user = await HttpContext.GetUserFromContext(_individualUserRepository);
                if (user == null)
                {
                    return new CreateTicketResponse(
                        StatusCodes.Status401Unauthorized,
                        "Unauthorized: Invalid User",
                        null
                    );
                }

                var response = await _service.CreateSupportTicket(user, model);
                return response;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new CreateTicketResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    null
                );
            }
        }

        [HttpGet]
        [Route("[action]")]
        [AuthPermission(Permissions.Support.View)]
        [SwaggerOperation(Tags = new[] { "Mobile" })]
        public async Task<ActionResult<GetTicketsResponse>> GetCurrentTicket()
        {
            try
            {
                var user = await HttpContext.GetUserFromContext(_individualUserRepository);
                if (user == null)
                {
                    return new GetTicketsResponse(
                       StatusCodes.Status401Unauthorized,
                       "Unauthorized: Invalid User",
                        null
                   );
                }

                var response = await _service.GetCurrentTicket(user);
                return response;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetTicketsResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    null
                );
            }
        }

        [HttpGet]
        [Route("[action]")]
        [AuthPermission(Permissions.Support.View)]
        [SwaggerOperation(Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<GetTicketsResponse>> GetOpenTickets([FromQuery] EntityFilter filter)
        {
            try
            {
                var response = await _service.GetOpenTickets(filter);
                return response;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetTicketsResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    null
                );
            }
        }

        [HttpGet]
        [Route("[action]")]
        [AuthPermission(Permissions.Support.View)]
        [SwaggerOperation(Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<GetTicketsResponse>> GetClosedTickets([FromQuery] EntityFilter filter)
        {
            try
            {
                var response = await _service.GetClosedTickets(filter);
                return response;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetTicketsResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    null
                );
            }
        }

        [HttpPost]
        [Route("[action]")]
        [AuthPermission(Permissions.Support.Create)]
        [SwaggerOperation(Tags = new[] { "Dashboard", "Mobile" })]
        public async Task<ActionResult<SuccessResponse>> ReplyTicket([FromBody] ReplyTicketDTO model)
        {
            try
            {
                var user = await HttpContext.GetUserFromContext(_individualUserRepository);
                var isAdmin = user == null;
                var response = await _service.ReplyTicket(isAdmin, model);
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

        [HttpPost]
        [Route("[action]")]
        [AuthPermission(Permissions.Support.View)]
        [SwaggerOperation(Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<SuccessResponse>> CloseSupportTicket([FromQuery] int ticketId)
        {
            try
            {
                var response = await _service.CloseSupportTicket(ticketId);
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
        [AuthPermission(Permissions.Support.Delete)]
        [SwaggerOperation(Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<SuccessResponse>> DeleteSupportTicket([FromQuery] int ticketId)
        {
            try
            {
                var response = await _service.DeleteSupportTicket(ticketId);
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
        [AuthPermission(Permissions.Support.Delete)]
        [SwaggerOperation(Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<SuccessResponse>> DeleteSupportType([FromQuery] int typeId)
        {
            try
            {
                var response = await _service.DeleteSupportType(typeId);
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
