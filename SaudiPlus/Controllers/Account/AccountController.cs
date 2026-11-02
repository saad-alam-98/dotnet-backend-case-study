using SaudiPlus.Response;
using SaudiPlus.Extensions;
using SaudiPlus.Controllers;
using Microsoft.AspNetCore.Mvc;
using SaudiPlus.Request.Account;
using SaudiPlus.Response.Account;
using SaudiPlus.Interfaces.Service;
using SaudiPlus.Interfaces.Repository;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.AspNetCore.Authorization;

namespace Controller.Account
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : BaseController<AccountController>
    {
        private readonly IAccountService _service;
        private readonly IIndividualUserRepository _individualUserRepository;
        public AccountController(
            ILogger<AccountController> logger,
            IAccountService _accountService,
            IIndividualUserRepository individualUserRepository
        ) : base(logger)
        {
            _service = _accountService;
            _individualUserRepository = individualUserRepository;
        }

        [HttpPost]
        [Route("[action]")]
        [SwaggerOperation(Tags = new[] { "Dashboard" , "Business" })]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequestDTO model)
        {
            try
            {
                var response = await _service.Login(model);
                return Ok(response);
            }
            catch (Exception ex)
            {
                Logger.LogError($"{ex.Message}");
                return new LoginResponse
                (
                    StatusCodes.Status500InternalServerError,
                    ex.Message,
                    null
                );
            }
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<ActionResult<ForgetPasswordResponse>> ForgetPassword([FromBody] ForgetPasswordDTO model)
        {
            try
            {
                var response = await _service.ForgetPasswordAsync(model);
                return Ok(response);
            }
            catch (Exception ex)
            {
                Logger.LogError($"{ex.Message}");
                return new ForgetPasswordResponse
                (
                    StatusCodes.Status500InternalServerError,
                    ex.Message,
                    null
                );
            }
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<ActionResult<SuccessResponse>> ValidateOtp([FromBody] ValidateOtpDTO model)
        {
            try
            {
                var response = await _service.ValidateOTPAsync(model);
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

        [HttpPost]
        [Route("[action]")]
        public async Task<ActionResult<SuccessResponse>> ChangePassword([FromBody] ChangePasswordDTO model)
        {
            try
            {
                var response = await _service.ChangePasswordAsync(model);
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

        [HttpPost]
        [Route("[action]")]
        [AuthPermission]
        public async Task<ActionResult<SuccessResponse>> ResetPassword([FromBody] ResetPasswordDTO model)
        {
            try
            {
                var response = await _service.ResetPasswordAsync(model);
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

        [HttpPost]
        [Route("[action]")]
        [AuthPermission]
        public async Task<ActionResult<SuccessResponse>> Logout([FromBody] LogoutDTO model)
        {
            try
            {
                var response = await _service.LogoutUser(model);
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

        [HttpPost]
        [Route("[action]")]
        public async Task<ActionResult<RefreshTokenResponse>> RefreshToken([FromBody] RefreshTokenDTO model)
        {
            try
            {
                var response = await _service.RefreshAccessToken(model);
                return Ok(response);
            }
            catch (Exception ex)
            {
                Logger.LogError($"{ex.Message}");
                return new RefreshTokenResponse
                (
                    StatusCodes.Status500InternalServerError,
                    ex.Message,
                    null
                );
            }
        }

        [HttpPost]
        [Route("[action]")]
        [AuthPermission]
        [SwaggerOperation(Tags = new[] { "Mobile" })]
        public async Task<ActionResult<SuccessResponse>> RegisterPhoneNumber([FromBody] RegisterPhoneNumberDTO model)
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

                var business = await _service.ConfirmPhoneRequest(user, model);
                return Ok(business);
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
        [AuthPermission]
        [SwaggerOperation(Tags = new[] { "Mobile" })]
        public async Task<ActionResult<SuccessResponse>> ConfirmPhoneOtp(ConfirmPhoneDTO model)
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

                var business = await _service.ConfirmPhoneOtp(user.Email, model);
                return Ok(business);
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
