using SaudiPlus.Response;
using SaudiPlus.Constants;
using SaudiPlus.Extensions;
using SaudiPlus.Request.Policy;
using Microsoft.AspNetCore.Mvc;
using SaudiPlus.Response.Policy;
using SaudiPlus.Interfaces.Service;
using SaudiPlus.Request.BusinessUser;
using SaudiPlus.Interfaces.Repository;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Authorization;
using Swashbuckle.AspNetCore.Annotations;

namespace SaudiPlus.Controllers.Policy
{
    [ApiController]
    [Route("api/[controller]")]
    public class PolicyController : BaseController<PolicyController>
    {
        private readonly IPolicyService _service;
        private readonly IBusinessRepository _repository;
        public PolicyController(
            IPolicyService service,
            IBusinessRepository repository,
            ILogger<PolicyController> logger
        ) : base(logger)
        {
            _service = service;
            _repository = repository;
        }

        [HttpPost]
        [Route("[action]")]
        [SwaggerOperation(Tags = new[] { "Dashboard" })]
        [AuthPermission(Permissions.TermsAndConditions.Create)]
        public async Task<ActionResult<GetPolicyResponse>> SetPolicy([FromBody] SetPolicyDTO model)
        {
            try
            {
                var response = await _service.SetPolicy(model);
                return Ok(response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetPolicyResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: null
                );
            }
        }

        [HttpGet]
        [Route("[action]")]
        [SwaggerOperation(Tags = new[] { "Dashboard" })]
        [AuthPermission(Permissions.TermsAndConditions.View)]
        public async Task<ActionResult<GetPolicyResponse>> GetPolicy([FromQuery] string type)
        {
            try
            {
                var Response = await _service.GetPolicy(type);
                return Ok(Response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetPolicyResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: null
                );
            }
        }

        [HttpGet]
        [Route("[action]")]
        [SwaggerOperation(Tags = new[] { "Mobile" })]
        [EnableRateLimiting("FixedWindowPolicy")]
        public async Task<ActionResult<GetPolicyResponse>> GetPolicyLocalized([FromQuery] string type)
        {
            try
            {
                var Response = await _service.GetPolicyLocalized(type);
                return Ok(Response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetPolicyResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: null
                );
            }
        }

        [HttpPost]
        [Route("[action]")]
        [SwaggerOperation(Tags = new[] { "Business" })]
        [AuthPermission(Permissions.TermsAndConditions.Create)]
        public async Task<ActionResult<GetPolicyResponse>> SetBusinessPolicy([FromBody] SetPolicyDTO model)
        {
            try
            {
                var business = await HttpContext.GetBusinessFromContext(_repository);
                if (business == null)
                {
                    return new GetPolicyResponse(
                       StatusCodes.Status401Unauthorized,
                       "Unauthorized: Invalid Business",
                        null
                   );
                }
                var response = await _service.SetBusinessPolicy(business.Id, model);
                return Ok(response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetPolicyResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: null
                );
            }
        }

        [HttpGet]
        [Route("[action]")]
        [SwaggerOperation(Tags = new[] { "Business" })]
        [AuthPermission(Permissions.TermsAndConditions.View)]
        public async Task<ActionResult<GetPolicyResponse>> GetBusinessPolicy([FromQuery] string type)
        {
            try
            {
                var business = await HttpContext.GetBusinessFromContext(_repository);
                if (business == null)
                {
                    return new GetPolicyResponse(
                       StatusCodes.Status401Unauthorized,
                       "Unauthorized: Invalid Business",
                        null
                   );
                }
                var Response = await _service.GetBusinessPolicy(business.Id, type);
                return Ok(Response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetPolicyResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: null
                );
            }
        }

        [HttpGet]
        [Route("[action]")]
        [SwaggerOperation(Tags = new[] { "Mobile" })]
        [AuthPermission(Permissions.TermsAndConditions.View)]
        public async Task<ActionResult<GetPolicyResponse>> GetBusinessPolicyLocalized(
            [FromQuery] Guid businessId, 
            [FromQuery] string type
        )
        {
            try
            {
                var Response = await _service.GetBusinessPolicyLocalized(businessId, type);
                return Ok(Response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetPolicyResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: null
                );
            }
        }

        [HttpPost]
        [Route("[action]")]
        [AuthPermission(Permissions.Contact.Create)]
        [SwaggerOperation(Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<SuccessResponse>> SetSocialMedia([FromBody] SetSocialMediaDto model)
        {
            try
            {
                var Response = await _service.SetSocialMedia(model);
                return Ok(Response);
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
        [AuthPermission(Permissions.Contact.Create)]
        [SwaggerOperation(Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<SuccessResponse>> SetContactUs([FromBody] SetContactDTO model)
        {
            try
            {
                var Response = await _service.SetContactUs(model);
                return Ok(Response);
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
        [AuthPermission(Permissions.Contact.View)]
        public async Task<ActionResult<GetContactUsResponse>> GetContactUs()
        {
            try
            {
                var Response = await _service.GetContactUs();
                return Ok(Response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetContactUsResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: null
                );
            }
        }
    }
}
