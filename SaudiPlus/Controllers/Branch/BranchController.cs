using SaudiPlus.Request;
using SaudiPlus.Response;
using SaudiPlus.Constants;
using SaudiPlus.Extensions;
using System.Security.Claims;
using SaudiPlus.Helper.Filter;
using SaudiPlus.Request.Branch;
using Microsoft.AspNetCore.Mvc;
using SaudiPlus.Response.Branch;
using SaudiPlus.Interfaces.Service;
using SaudiPlus.Interfaces.Repository;
using Microsoft.AspNetCore.Authorization;
using Swashbuckle.AspNetCore.Annotations;

namespace SaudiPlus.Controllers.Branch
{
    [ApiController]
    [Route("api/[controller]")]
    public class BranchController : BaseController<BranchController>
    {
        private IBranchService _service;
        private readonly IBusinessRepository _businessRepository;
        public BranchController(
            IBranchService service,
            IBusinessRepository businessRepository,
            ILogger<BranchController> logger
        ) : base(logger)
        {
            _service = service;
            _businessRepository = businessRepository;
        }

        [HttpPost]
        [Route("[action]")]
        [AuthPermission(Permissions.Branch.Create)]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<ActionResult<GetBranchResponse>> AdminCreateBranch([FromBody] AdminCreateBranchDTO model)
        {
            try
            {
                var response = await _service.CreateBranch(model.BusinessId, model);
                return response;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetBranchResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    null
                );
            }
        }

        [HttpPost]
        [Route("[action]")]
        [AuthPermission(Permissions.Branch.Create)]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<ActionResult<GetBranchResponse>> CreateBranch([FromBody] BusinessCreateBranchDTO model)
        {
            try
            {
                var business = await HttpContext.GetBusinessFromContext(_businessRepository);
                if (business == null)
                {
                    return new GetBranchResponse(
                        StatusCodes.Status401Unauthorized,
                        "Unauthorized: Invalid business",
                        null
                    );
                }
                var response = await _service.CreateBranch(business.Id, model);
                return response;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetBranchResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    null
                );
            }
        }

        [HttpGet]
        [Route("[action]")]
        [AuthPermission(Permissions.Branch.View)]
        public async Task<ActionResult<GetBranchResponse>> GetBranchById([FromQuery] int branchId)
        {
            try
            {
                var response = await _service.GetBranchById(branchId);
                return response;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetBranchResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: null
                );
            }
        }

        [HttpGet]
        [Route("[action]")]
        [AuthPermission(Permissions.Branch.View)]
        [SwaggerOperation(Tags = new[] { "Business", "Dashboard" })]
        public async Task<ActionResult<GetBranchQRCodeResponse>> GetBranchQR([FromQuery] int branchId)
        {
            try
            {
                var response = await _service.GetBranchQR(branchId);
                return response;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetBranchQRCodeResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: null
                );
            }
        }

        [HttpGet]
        [Route("[action]")]
        [AuthPermission(Permissions.Branch.Edit)]
        [SwaggerOperation(Tags = new[] { "Business" })]
        public async Task<ActionResult<SuccessResponse>> SetBranchQR([FromQuery] int branchId)
        {
            try
            {
                var business = await HttpContext.GetBusinessFromContext(_businessRepository);
                if (business == null)
                {
                    return new SuccessResponse(
                        StatusCodes.Status401Unauthorized,
                        "Unauthorized: Invalid business",
                        new SuccessResponsePayload { success = false }
                    );
                }
                var response = await _service.SetBranchQR(business.Id, branchId);
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
        [AuthPermission(Permissions.Branch.View)]
        [SwaggerOperation(Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<GetBranchesResponse>> GetBusinessBranch([FromQuery] Guid businessId)
        {
            try
            {
                var business = await _service.GetBusinessBranches(businessId);
                return Ok(business);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetBranchesResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: null
                );
            }
        }

        [HttpGet]
        [Route("[action]")]
        [AuthPermission(Permissions.Branch.View)]
        [SwaggerOperation(Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<GetAllBranchResponse>> GetAllBranches([FromQuery] EntityFilter filter)
        {
            try
            {
                var business = await _service.GetAllBranches(filter);
                return Ok(business);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetAllBranchResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: null
                );
            }
        }

        [HttpGet]
        [Route("[action]")]
        [AuthPermission(Permissions.Branch.View)]
        [SwaggerOperation(Tags = new[] { "Business" })]
        public async Task<ActionResult<GetBranchesResponse>> GetBranchByFilter([FromQuery] EntityFilter filter)
        {
            try
            {
                var business = await HttpContext.GetBusinessFromContext(_businessRepository);
                if (business == null)
                {
                    return new GetBranchesResponse
                    (
                        statusCode: StatusCodes.Status500InternalServerError,
                        message: "Unauthorized: Invalid business",
                        payload: null
                    );
                }
                var response = await _service.GetBranchByFilter(business.Id, filter);
                return Ok(response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetBranchesResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: null
                );
            }
        }

        [HttpGet]
        [Route("[action]")]
        [AuthPermission(Permissions.Branch.View)]
        [SwaggerOperation(Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<GetBranchesResponse>> GetBusinessBranchByFilter(
            [FromQuery] Guid businessId,
            [FromQuery] EntityFilter filter)
        {
            try
            {
                var response = await _service.GetBranchByFilter(businessId, filter);
                return Ok(response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetBranchesResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: null
                );
            }
        }

        [HttpGet]
        [Route("[action]")]
        [AuthPermission(Permissions.Branch.View)]
        [SwaggerOperation(Tags = new[] { "Business" })]
        public async Task<ActionResult<GetBranchesResponse>> GetBranch()
        {
            try
            {
                var userIdClaim = HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return new GetBranchesResponse(
                        StatusCodes.Status401Unauthorized,
                        "Unauthorized: Unable to extract user ID.",
                        null
                    );
                }

                if (!Guid.TryParse(userIdClaim.Value, out Guid userId))
                {
                    return new GetBranchesResponse(
                        StatusCodes.Status401Unauthorized,
                        "Unauthorized: Invalid user ID format.",
                        null
                    );
                }

                var business = await _service.GetBranch(userId);
                return Ok(business);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetBranchesResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: null
                );
            }
        }

        [HttpGet]
        [Route("[action]")]
        [AuthPermission(Permissions.Branch.View)]
        [SwaggerOperation(Tags = new[] { "Mobile" })]
        public async Task<ActionResult<GetBranchesResponse>> GetBusinessActiveBranch([FromQuery] Guid businessId)
        {
            try
            {
                var business = await _service.GetBusinessActiveBranches(businessId);
                return Ok(business);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetBranchesResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: null
                );
            }
        }


        [HttpPatch]
        [Route("[action]")]
        [AuthPermission(Permissions.Branch.Edit)]
        [SwaggerOperation(Tags = new[] { "Dashboard", "Business" })]
        public async Task<ActionResult<GetBranchResponse>> EditBranch([FromBody] EditBranchDTO model)
        {
            try
            {
                var response = await _service.EditBranch(model);
                return response;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetBranchResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                   null
                );
            }
        }

        [HttpPatch]
        [Route("[action]")]
        [AuthPermission(Permissions.Branch.Edit)]
        [SwaggerOperation(Tags = new[] { "Dashboard", "Business" })]
        public async Task<ActionResult<SuccessResponse>> ToggleBranchStatus([FromBody] ToggleStatusDTO<int> model)
        {
            try
            {
                var response = await _service.ToggleBranchStatus(model);
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
        [AuthPermission(Permissions.Branch.Delete)]
        [SwaggerOperation(Tags = new[] { "Dashboard", "Business" })]
        public async Task<ActionResult<SuccessResponse>> DeleteBranch([FromQuery] int branchId)
        {
            try
            {
                var response = await _service.DeleteBranch(branchId);
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
        [AuthPermission(Permissions.Branch.Edit)]
        [SwaggerOperation(Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<SuccessResponse>> SyncBranchRating()
        {
            try
            {
                var response = await _service.SyncBranchRating();
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
