using SaudiPlus.Response;
using SaudiPlus.Constants;
using Microsoft.AspNetCore.Mvc;
using SaudiPlus.Request.Contract;
using SaudiPlus.Response.Contract;
using SaudiPlus.Interfaces.Service;
using Swashbuckle.AspNetCore.Annotations;

namespace SaudiPlus.Controllers.Contract
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContractController : BaseController<ContractController>
    {
        private readonly IContractService _service;
        public ContractController(
            IContractService ContractService,
            ILogger<ContractController> logger
        ) : base(logger)
        {
            this._service = ContractService;
        }

        [HttpPost]
        [Route("[action]")]
        [AuthPermission(Permissions.Contract.View)]
        [SwaggerOperation(Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<SuccessResponse>> AddContract([FromBody] CreateContractDTO model)
        {
            try
            {
                var response = await _service.AddContract(model);
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
        [AuthPermission(Permissions.Contract.Edit)]
        [SwaggerOperation(Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<SuccessResponse>> EditContract([FromBody] EditContractDTO model)
        {
            try
            {
                var response = await _service.EditContract(model);
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
        [AuthPermission(Permissions.Contract.View)]
        [SwaggerOperation(Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<GetContractResponse>> GetContract([FromQuery] Guid businessId)
        {
            try
            {
                var Response = await _service.GetContract(businessId);
                return Ok(Response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetContractResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: null
                );
            }
        }

        [HttpDelete]
        [Route("[action]")]
        [AuthPermission(Permissions.Contract.Delete)]
        [SwaggerOperation(Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<SuccessResponse>> DeleteContract([FromQuery] int ContractId)
        {
            try
            {
                var response = await _service.DeleteContract(ContractId);
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
