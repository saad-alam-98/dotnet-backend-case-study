using SaudiPlus.Response;
using SaudiPlus.Constants;
using SaudiPlus.Extensions;
using SaudiPlus.Helper.Filter;
using Microsoft.AspNetCore.Mvc;
using SaudiPlus.Request.Invoice;
using SaudiPlus.Response.Invoice;
using SaudiPlus.Interfaces.Service;
using SaudiPlus.Interfaces.Repository;
using Swashbuckle.AspNetCore.Annotations;

namespace SaudiPlus.Controllers.Invoice
{
    [ApiController]
    [Route("api/[controller]")]
    public class InvoiceController : BaseController<InvoiceController>
    {
        private IInvoiceService _service;
        private IBusinessRepository _businessRepository;
        public InvoiceController(
            IInvoiceService service,
            IBusinessRepository businessRepository,
            ILogger<InvoiceController> logger
        ) : base(logger)
        {
            _businessRepository = businessRepository;
            _service = service;
        }

        [HttpGet]
        [Route("[action]")]
        [AuthPermission(Permissions.Invoice.Create, Permissions.Branch.View)]
        [SwaggerOperation(Tags = new[] { "Business" })]
        public async Task<ActionResult<GetBranchForInvoice>> GetBranchForInvoice()
        {
            try
            {
                var business = await HttpContext.GetBusinessFromContext(_businessRepository);
                if (business == null)
                {
                    return new GetBranchForInvoice(
                        StatusCodes.Status401Unauthorized,
                        "Unauthorized: Unable to extract user ID.",
                        null
                    );
                }

                var response = await _service.GetBranchForInvoice(business.Id);
                return response;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetBranchForInvoice
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    null
                );
            }
        }

        [HttpPost]
        [Route("[action]")]
        [AuthPermission(Permissions.Invoice.Create)]
        [SwaggerOperation(Tags = new[] { "Business" })]
        public async Task<ActionResult<CreateInvoiceResponse>> CreateInvoice([FromBody] CreateInvoiceDTO model)
        {
            try
            {
                var business = await HttpContext.GetBusinessFromContext(_businessRepository);
                if (business == null)
                {
                    return new CreateInvoiceResponse(
                        StatusCodes.Status401Unauthorized,
                        "Unauthorized: Unable to extract user ID.",
                        null
                    );
                }

                var response = await _service.CreateInvoice(business, model);
                return response;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new CreateInvoiceResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    null
                );
            }
        }

        [HttpPost]
        [Route("[action]")]
        [AuthPermission(Permissions.Invoice.Create)]
        [SwaggerOperation(Tags = new[] { "Business" })]
        public async Task<ActionResult<SuccessResponse>> SaveInvoice([FromBody] SaveInvoiceDTO model)
        {
            try
            {
                var business = await HttpContext.GetBusinessFromContext(_businessRepository);
                if (business == null)
                {
                    return new SuccessResponse(
                        StatusCodes.Status401Unauthorized,
                        "Unauthorized: Unable to extract user ID.",
                        new SuccessResponsePayload { success = false }
                    );
                }

                var response = await _service.SaveInvoice(business, model);
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
        [AuthPermission(Permissions.Invoice.View)]
        [SwaggerOperation(Tags = new[] { "Business" })]
        public async Task<ActionResult<GetInvoicesResponse>> GetInvoiceByFilter([FromQuery] EntityFilter filter)
        {
            try
            {
                var business = await HttpContext.GetBusinessFromContext(_businessRepository);
                if (business == null)
                {
                    return new GetInvoicesResponse(
                        StatusCodes.Status401Unauthorized,
                        "Unauthorized: Unable to extract user ID.",
                        null
                    );
                }

                var response = await _service.GetInvoiceByFilter(business.Id, filter);
                return response;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetInvoicesResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    null
                );
            }
        }

        [HttpGet]
        [Route("[action]")]
        [AuthPermission(Permissions.Invoice.Create)]
        [SwaggerOperation(Tags = new[] { "Business" })]
        public async Task<ActionResult<GetOfferForInvoiceResponse>> GetBusinessInvoiceOffers()
        {
            try
            {
                var business = await HttpContext.GetBusinessFromContext(_businessRepository);
                if (business == null)
                {
                    return new GetOfferForInvoiceResponse(
                        StatusCodes.Status401Unauthorized,
                        "Unauthorized: Unable to extract user ID.",
                        null
                    );
                }

                var response = await _service.GetBusinessInvoiceOffers(business.Id);
                return response;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetOfferForInvoiceResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    null
                );
            }
        }
    }
}
