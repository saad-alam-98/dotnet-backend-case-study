using SaudiPlus.Helper.Filter;
using Microsoft.AspNetCore.Mvc;
using SaudiPlus.Response.Wallet;
using SaudiPlus.Interfaces.Service;
using Microsoft.AspNetCore.Authorization;
using Swashbuckle.AspNetCore.Annotations;
using SaudiPlus.Constants;

namespace SaudiPlus.Controllers.Wallet
{
    [ApiController]
    [Route("api/[controller]")]
    public class WalletController : BaseController<WalletController>
    {
        private IWalletService _service;
        public WalletController(
            IWalletService service,
            ILogger<WalletController> logger
        ) : base(logger)
        {
            _service = service;
        }

        [HttpGet]
        [Route("[action]")]
        [AuthPermission(Permissions.Reports.View)]
        [SwaggerOperation(Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<GetBusinessWalletResponse>> GetBusinessWallet(
            [FromQuery] Guid businessId,
            [FromQuery] int year,
            [FromQuery] int month,
            [FromQuery] EntityFilter filter
        )
        {
            try
            {
                var response = await _service.GetBusinessWallet(businessId, year, month, filter);
                return response;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetBusinessWalletResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    null
                );
            }
        }
    }
}
