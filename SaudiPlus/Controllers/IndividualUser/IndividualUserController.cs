using SaudiPlus.Request;
using SaudiPlus.Response;
using SaudiPlus.Constants;
using SaudiPlus.Extensions;
using SaudiPlus.Helper.Filter;
using SaudiPlus.Response.User;
using Microsoft.AspNetCore.Mvc;
using SaudiPlus.Request.Account;
using SaudiPlus.Interfaces.Service;
using SaudiPlus.Interfaces.Repository;
using SaudiPlus.Request.IndividualUser;
using SaudiPlus.Response.IndividualUser;
using Microsoft.AspNetCore.Authorization;
using Swashbuckle.AspNetCore.Annotations;

namespace SaudiPlus.Controllers.IndividualUser
{
    [ApiController]
    [Route("api/[controller]")]
    public class IndividualUserController : BaseController<IndividualUserController>
    {
        private readonly IIndividualUserService _service;
        private readonly IIndividualUserRepository _individualUserRepository;
        public IndividualUserController(
            ILogger<IndividualUserController> logger,
            IIndividualUserService individualUser,
            IIndividualUserRepository individualUserRepository
        ) : base(logger)
        {
            _service = individualUser;
            _individualUserRepository = individualUserRepository;
        }

        [HttpPost]
        [Route("[action]")]
        [SwaggerOperation(Tags = new[] { "Mobile" })]
        [AllowAnonymous]
        public async Task<ActionResult<RegisterUserResponse>> RegisterUser([FromBody] RegisterUserDTO model)
        {
            try
            {
                var response = await _service.RegisterUser(model);
                return Ok(response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new RegisterUserResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: null
                );
            }
        }

        [HttpPost]
        [Route("[action]")]
        [AuthPermission(Permissions.User.Create)]
        [SwaggerOperation(Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<UserCreateResponse>> AdminCreateIndividualUser(CreateUserDTO model)
        {
            try
            {
                var user = await _service.CreateIndividualUser(model);
                return Ok(user);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new UserCreateResponse
                (
                    statusCode: StatusCodes.Status200OK,
                    message: ex.Message,
                    payload: null
                );
            }
        }

        [HttpPost]
        [Route("[action]")]
        [SwaggerOperation(Tags = new[] { "Mobile" })]
        [AllowAnonymous]
        public async Task<ActionResult<UserCreateResponse>> CreateIndividualUser(CreateUserDTO model)
        {
            try
            {
                var user = await _service.CreateIndividualUser(model);
                return Ok(user);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new UserCreateResponse
                (
                    statusCode: StatusCodes.Status200OK,
                    message: ex.Message,
                    payload: null
                );
            }
        }


        [HttpPost]
        [Route("[action]")]
        [SwaggerOperation(Tags = new[] { "Mobile" })]
        public async Task<ActionResult<IndividualUserLoginResponse>> LoginUser([FromBody] LoginRequestDTO model)
        {
            try
            {
                var response = await _service.LoginUser(model);
                return Ok(response);
            }
            catch (Exception ex)
            {
                Logger.LogError($"{ex.Message}");
                return new IndividualUserLoginResponse
                (
                    StatusCodes.Status500InternalServerError,
                    ex.Message,
                    null
                );
            }
        }

        [HttpPost]
        [Route("[action]")]
        [SwaggerOperation(Tags = new[] { "Mobile" })]
        public async Task<ActionResult<IndividualUserLoginResponse>> GuestLogin([FromBody] GuestLoginDTO model)
        {
            try
            {
                var response = await _service.GuestUserLogin(model);
                return Ok(response);
            }
            catch (Exception ex)
            {
                Logger.LogError($"{ex.Message}");
                return new IndividualUserLoginResponse
                (
                    StatusCodes.Status500InternalServerError,
                    ex.Message,
                    null
                );
            }
        }

        [HttpPatch]
        [Route("[action]")]
        [AuthPermission(Permissions.User.Edit)]
        [SwaggerOperation(Tags = new[] { "Dashboard", "Mobile" })]
        public async Task<ActionResult<UserEditResponse>> EditUser([FromBody] EditUserDTO model)
        {
            try
            {
                var user = await HttpContext.GetUserFromContext(_individualUserRepository);
                if (user == null)
                {
                    return new UserEditResponse(
                       StatusCodes.Status401Unauthorized,
                       "Unauthorized: Invalid User",
                       null
                   );
                }

                var response = await _service.EditUser(user.Id, model);
                return Ok(response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new UserEditResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: null
                );
            }
        }

        [HttpPatch]
        [Route("[action]")]
        [AuthPermission(Permissions.User.Edit)]
        [SwaggerOperation(Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<SuccessResponse>> ToggleUserStatus([FromBody] ToggleStatusDTO<Guid> model)
        {
            try
            {
                var response = await _service.ToggleUserStatus(model);
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
        [AuthPermission]
        [SwaggerOperation(Tags = new[] { "Mobile" })]
        public async Task<ActionResult<GetUserResponse>> GetUserInfo()
        {
            try
            {
                var user = await HttpContext.GetUserFromContext(_individualUserRepository);
                if (user == null)
                {
                    return new GetUserResponse(
                       StatusCodes.Status401Unauthorized,
                       "Unauthorized: Invalid User",
                       null
                   );
                }

                var response = await _service.GetUserById(user.Id);
                return Ok(response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetUserResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: null
                );
            }
        }

        [HttpGet]
        [Route("[action]")]
        [AuthPermission(Permissions.User.View)]
        [SwaggerOperation(Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<GetUserResponse>> AdminGetUserInfo([FromQuery] Guid userId)
        {
            try
            {
                var response = await _service.GetUserById(userId);
                return Ok(response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetUserResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: null
                );
            }
        }

        [HttpGet]
        [Route("[action]")]
        [AuthPermission(Permissions.User.View)]
        [SwaggerOperation(Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<GetUsersResponse>> GetUserByFilter([FromQuery] EntityFilter filter)
        {
            try
            {
                var business = await _service.GetUserByFilter(filter);
                return Ok(business);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetUsersResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: null
                );
            }
        }

        [HttpGet]
        [Route("[action]")]
        [AuthPermission]
        [SwaggerOperation(Tags = new[] { "Mobile" })]
        public async Task<ActionResult<GetOrdersResponse>> GetOrders([FromQuery] EntityFilter filter)
        {
            try
            {
                var user = await HttpContext.GetUserFromContext(_individualUserRepository);
                if (user == null)
                {
                    return new GetOrdersResponse(
                       StatusCodes.Status401Unauthorized,
                       "Unauthorized: Invalid User",
                       null
                   );
                }

                var response = await _service.GetOrders(user.Id, filter);
                return Ok(response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetOrdersResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: null
                );
            }
        }

        [HttpGet]
        [Route("[action]")]
        [AuthPermission]
        [SwaggerOperation(Tags = new[] { "Mobile" })]
        public async Task<ActionResult<GetPointsResponse>> GetPoints()
        {
            try
            {
                var user = await HttpContext.GetUserFromContext(_individualUserRepository);
                if (user == null)
                {
                    return new GetPointsResponse(
                       StatusCodes.Status401Unauthorized,
                       "Unauthorized: Invalid User",
                       null
                   );
                }

                var response = await _service.GetPoints(user);
                return Ok(response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetPointsResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: null
                );
            }
        }

        [HttpPatch]
        [Route("[action]")]
        [AuthPermission(Permissions.User.Edit)]
        [SwaggerOperation(Tags = new[] { "Mobile" })]
        public async Task<ActionResult<SuccessResponse>> SetAvatar([FromForm] SetUserAvatarDTO model)
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

                var response = await _service.SetAvatar(user, model);
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
        [AuthPermission(Permissions.User.View)]
        [SwaggerOperation(Tags = new[] { "Mobile" })]
        public async Task<ActionResult<GetAvatarResponse>> GetAvatar()
        {
            try
            {
                var user = await HttpContext.GetUserFromContext(_individualUserRepository);
                if (user == null)
                {
                    return new GetAvatarResponse(
                       StatusCodes.Status401Unauthorized,
                       "Unauthorized: Invalid User",
                        null
                   );
                }

                var response = await _service.GetAvatar(user);
                return Ok(response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetAvatarResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: null
                );
            }
        }

        [HttpGet]
        [Route("[action]")]
        [AuthPermission(Permissions.User.Edit)]
        [SwaggerOperation(Tags = new[] { "Mobile" })]
        public async Task<ActionResult<SuccessResponse>> SetDeviceToken([FromQuery] string token)
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

                var response = await _service.SetDeviceToken(user, token);
                return Ok(response);
            }
            catch (Exception ex)
            {
                Logger.LogError($"{ex.Message}");
                return new SuccessResponse
                (
                    StatusCodes.Status500InternalServerError,
                    ex.Message,
                     new SuccessResponsePayload { success = false }
                );
            }
        }

        [HttpPatch]
        [Route("[action]")]
        [AuthPermission(Permissions.User.Edit)]
        [SwaggerOperation(Tags = new[] { "Mobile" })]
        public async Task<ActionResult<SuccessResponse>> SetLanguage([FromBody] SetLanguageDTO model)
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

                var response = await _service.SetLanguage(user, model);
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
        [AuthPermission(Permissions.User.Edit)]
        [SwaggerOperation(Tags = new[] { "Mobile" })]
        public async Task<ActionResult<SuccessResponse>> ToggleFavourite([FromQuery] Guid businessId, [FromQuery] bool isFavourite)
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

                var response = await _service.ToggleFavourite(user.Id, businessId, isFavourite);
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
        [AuthPermission]
        [SwaggerOperation(Tags = new[] { "Mobile" })]
        public async Task<ActionResult<SuccessResponse>> GetFavouriteBusiness([FromQuery] LocationDTO? location)
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

                var response = await _service.GetFavouriteBusiness(user.Id, location);
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
        [AuthPermission(Permissions.User.Delete)]
        [SwaggerOperation(Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<SuccessResponse>> DeleteUser([FromQuery] Guid userId)
        {
            try
            {
                var response = await _service.DeleteUser(userId);
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
        [AuthPermission]
        [SwaggerOperation(Tags = new[] { "Mobile" })]
        public async Task<ActionResult<SuccessResponse>> DeleteAccount()
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

                var response = await _service.DeleteAccount(user);
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
