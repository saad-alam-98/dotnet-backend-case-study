using SaudiPlus.Request.Auth;
using SaudiPlus.Response.User;
using Microsoft.AspNetCore.Mvc;
using SaudiPlus.Interfaces.Service;
using Microsoft.AspNetCore.Authorization;
using Swashbuckle.AspNetCore.Annotations;

namespace SaudiPlus.Controllers.Auth
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : BaseController<AuthController>
    {
        private readonly IAuthService _service;
        public AuthController(
            IAuthService service,
            ILogger<AuthController> logger
        ) : base(logger)
        {
            _service = service;
        }

        [HttpPost]
        [Route("[action]")]
        [SwaggerOperation(Tags = new[] { "Mobile" })]
        [AllowAnonymous]
        public async Task<ActionResult<RegisterUserResponse>> GoogleLogin([FromBody] GoogleAuthRequestDTO model)
        {
            try
            {
                var response = await _service.GetOrRegisterGoogleUser(model);
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
        [SwaggerOperation(Tags = new[] { "Mobile" })]
        [AllowAnonymous]
        public async Task<ActionResult<RegisterUserResponse>> AppleLogin([FromBody] AppleAuthRequestDTO model)
        {
            try
            {
                var response = await _service.GetOrRegisterGoogleAppleUser(model);
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
    }
}
