using SaudiPlus.Constants;
using SaudiPlus.Helper.Filter;
using Microsoft.AspNetCore.Mvc;
using SaudiPlus.Response.Report;
using SaudiPlus.Interfaces.Service;
using SaudiPlus.Response.BusinessUser;
using Swashbuckle.AspNetCore.Annotations;

namespace SaudiPlus.Controllers.Report
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportController : BaseController<ReportController>
    {
        private readonly IReportService _service;
        public ReportController(
            IReportService service,
            ILogger<ReportController> logger
        ) : base(logger)
        {
            _service = service;
        }

        [HttpGet]
        [Route("[action]")]
        [AuthPermission(Permissions.Reports.View)]
        [SwaggerOperation(Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<GetStoreReportResponse>> GetStoreReport([FromQuery] DateEntityFilter filter)
        {
            try
            {
                var response = await _service.GetStoreReport(filter);
                return Ok(response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetStoreReportResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: null
                );
            }
        }

        [HttpGet]
        [Route("[action]")]
        [AuthPermission(Permissions.Reports.View)]
        [SwaggerOperation(Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<GetBranchReportResponse>> GetBranchReport([FromQuery] DateEntityFilter filter)
        {
            try
            {
                var response = await _service.GetBranchReport(filter);
                return Ok(response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetBranchReportResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: null
                );
            }
        }

        [HttpGet]
        [Route("[action]")]
        [AuthPermission(Permissions.Reports.View)]
        [SwaggerOperation(Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<GetDashboardReportResponse>> GetDashboardReport()
        {
            try
            {
                var response = await _service.GetDashboardReport();
                return Ok(response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetDashboardReportResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: null
                );
            }
        }

        [HttpGet]
        [Route("[action]")]
        [AuthPermission(Permissions.Reports.View)]
        [SwaggerOperation(Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<GetOfferRedeemStateResponse>> GetOfferUsage([FromQuery] int year)
        {
            try
            {
                var response = await _service.GetOfferUsage(year);
                return Ok(response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetOfferRedeemStateResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: null
                );
            }
        }

        [HttpGet]
        [Route("[action]")]
        [AuthPermission(Permissions.Reports.View)]
        [SwaggerOperation(Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<GetBusinessDetailsForDashboardResponse>> GetDashboardBusinessDetail([FromQuery] EntityFilter filter)
        {
            try
            {
                var response = await _service.GetDashboardBusinessDetail(filter);
                return Ok(response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetBusinessDetailsForDashboardResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: null
                );
            }
        }

        [HttpGet]
        [Route("[action]")]
        [AuthPermission(Permissions.Reports.View)]
        [SwaggerOperation(Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<GetBranchDetailsForDashboardResponse>> GetDashboardBranchDetail()
        {
            try
            {
                var response = await _service.GetDashboardBranchDetail();
                return Ok(response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetBranchDetailsForDashboardResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: null
                );
            }
        }

        [HttpGet]
        [Route("[action]")]
        [AuthPermission(Permissions.Reports.View)]
        [SwaggerOperation(Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<GetOfferUsageDetailsByMonthResponse>> GetDashboardOfferUsageByMonth(
            [FromQuery] int month, 
            [FromQuery] int year,
            [FromQuery] bool IsOnline = false)
        {
            try
            {
                var response = await _service.GetDashboardOfferUsageByMonth(month, year,IsOnline);
                return Ok(response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetOfferUsageDetailsByMonthResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: null
                );
            }
        }

        [HttpGet]
        [Route("[action]")]
        [AuthPermission(Permissions.Reports.View)]
        [SwaggerOperation(Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<GetOfferDashboardDetailsByTypeRespose>> GetDashboardOfferDetailsByType(
            [FromQuery] string type,
            [FromQuery] DateEntityFilter filter,
            [FromQuery] bool isOnline = false
        )
        {
            try
            {
                var response = await _service.GetDashboardOfferDetailsByType(type, filter, isOnline);
                return Ok(response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetOfferDashboardDetailsByTypeRespose
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: null
                );
            }
        }

        [HttpGet]
        [Route("[action]")]
        [AuthPermission(Permissions.Reports.View)]
        [SwaggerOperation(Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<GetOfferDashboardDetailsByTypeRespose>> GetDashboardUsageBusinessFilter(
            [FromQuery] Guid businessId,
            [FromQuery] DateEntityFilter filter
        )
        {
            try
            {
                var response = await _service.GetDashboardUsageBusinessFilter(businessId, filter);
                return Ok(response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetOfferDashboardDetailsByTypeRespose
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: null
                );
            }
        }

        [HttpGet]
        [Route("[action]")]
        [AuthPermission(Permissions.Reports.View)]
        [SwaggerOperation(Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<GetOfferDashboardDetailsByTypeRespose>> GetDashboardUsageOfferFilter(
            [FromQuery] int offerId,
            [FromQuery] DateEntityFilter filter
        )
        {
            try
            {
                var response = await _service.GetDashboardUsageOfferFilter(offerId, filter);
                return Ok(response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetOfferDashboardDetailsByTypeRespose
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: null
                );
            }
        }
    }
}
