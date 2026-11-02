using SaudiPlus.Request;
using SaudiPlus.Response;
using SaudiPlus.Constants;
using SaudiPlus.Extensions;
using System.Security.Claims;
using SaudiPlus.Request.Offer;
using SaudiPlus.Helper.Filter;
using Microsoft.AspNetCore.Mvc;
using SaudiPlus.Response.Offer;
using SaudiPlus.Response.Category;
using SaudiPlus.Interfaces.Service;
using SaudiPlus.Interfaces.Repository;
using Swashbuckle.AspNetCore.Annotations;

namespace SaudiPlus.Controllers.Offer
{
    [ApiController]
    [Route("api/[controller]")]
    public class OfferController : BaseController<OfferController>
    {
        private IOfferService _service;
        private IBusinessRepository _businessRepository;
        private IIndividualUserRepository _individualUserRepository;
        public OfferController(
            IOfferService service,
            IBusinessRepository businessRepository,
            IIndividualUserRepository individualUserRepository,
            ILogger<OfferController> logger
        ) : base(logger)
        {
            _service = service;
            _businessRepository = businessRepository;
            _individualUserRepository = individualUserRepository;
        }

        [HttpPost]
        [Route("[action]")]
        [AuthPermission(Permissions.Offer.Create)]
        [SwaggerOperation(Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<GetOfferResponse>> AdminCreateOffer([FromForm] AdminCreateOfferDTO model)
        {
            try
            {
                var response = await _service.CreateOffer(model.BusinessId, model);
                return response;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetOfferResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    null
                );
            }
        }

        [HttpPost]
        [Route("[action]")]
        [AuthPermission(Permissions.Offer.Create)]
        [SwaggerOperation(Tags = new[] { "Business" })]
        public async Task<ActionResult<GetOfferResponse>> CreateOffer([FromForm] CreateOfferDTO model)
        {
            try
            {
                var business = await HttpContext.GetBusinessFromContext(_businessRepository);
                if (business == null)
                {
                    return new GetOfferResponse(
                        StatusCodes.Status401Unauthorized,
                        "Unauthorized: Invalid business",
                        null
                    );
                }
               
                var response = await _service.CreateOffer(business.Id, model);
                return response;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetOfferResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    null
                );
            }
        }

        [HttpGet]
        [Route("[action]")]
        [AuthPermission(Permissions.Offer.View)]
        public async Task<ActionResult<GetOfferResponse>> GetOfferById([FromQuery] int offerId, [FromQuery] LocationDTO? location)
        {
            try
            {
                var response = await _service.GetOfferById(offerId, location);
                return response;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetOfferResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: null
                );
            }
        }


        [HttpPatch]
        [Route("[action]")]
        [AuthPermission(Permissions.Offer.Edit)]
        [SwaggerOperation(Tags = new[] { "Dashboard", "Business" })]
        public async Task<ActionResult<GetOfferResponse>> EditOffer([FromForm] EditOfferDTO model)
        {
            try
            {
                var response = await _service.EditOffer(model);
                return response;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetOfferResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                   null
                );
            }
        }

        [HttpPatch]
        [Route("[action]")]
        [AuthPermission(Permissions.Offer.Edit)]
        [SwaggerOperation(Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<SuccessResponse>> ToggleOfferStatus([FromBody] ToggleStatusDTO<int> model)
        {
            try
            {
                var response = await _service.ToggleOfferStatus(model);
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
        [AuthPermission(Permissions.Offer.View)]
        [SwaggerOperation(Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<GetOffersResponse>> AdminGetBusinessOffers(
            [FromQuery] Guid businessId,
            [FromQuery] EntityFilter filter
        )
        {
            try
            {
                var business = await _service.GetBusinessOffers(businessId, filter);
                return Ok(business);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetOffersResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: null
                );
            }
        }

        [HttpGet]
        [Route("[action]")]
        [AuthPermission(Permissions.Offer.View)]
        [SwaggerOperation(Tags = new[] { "Business" })]
        public async Task<ActionResult<GetOffersResponse>> GetBusinessOffers(
            [FromQuery] EntityFilter filter
        )
        {
            try
            {
                var business = await HttpContext.GetBusinessFromContext(_businessRepository);
                if (business == null)
                {
                    return new GetOffersResponse(
                        StatusCodes.Status401Unauthorized,
                        "Unauthorized: Invalid business",
                        null
                    );
                }

                var offers = await _service.GetBusinessOffers(business.Id, filter);
                return Ok(offers);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetOffersResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: null
                );
            }
        }

        [HttpGet]
        [Route("[action]")]
        [AuthPermission(Permissions.Offer.View)]
        [SwaggerOperation(Tags = new[] { "Mobile" })]
        public async Task<ActionResult<GetOffersResponse>> GetBusinessActiveOffers([FromQuery] Guid businessId, [FromQuery] LocationDTO? location)
        {
            try
            {
                var user = await HttpContext.GetUserFromContext(_individualUserRepository);
                if (user == null)
                {
                    return new GetOffersResponse(
                        StatusCodes.Status401Unauthorized,
                        "Unauthorized: Invalid User",
                        null
                    );
                }

                var business = await _service.GetBusinessActiveOffers(user, businessId, location);
                return Ok(business);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetOffersResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: null
                );
            }
        }

        [HttpGet]
        [Route("[action]")]
        [AuthPermission(Permissions.Offer.View)]
        [SwaggerOperation(Tags = new[] { "Mobile" })]
        public async Task<ActionResult<GetOffersResponse>> GetTopOffers([FromQuery] LocationDTO? location)
        {
            try
            {
                var business = await _service.GetTopOffers(location);
                return Ok(business);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetOffersResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: null
                );
            }
        }

        [HttpPost]
        [Route("[action]")]
        [AuthPermission(Permissions.Offer.View)]
        [SwaggerOperation(Tags = new[] { "Mobile" })]
        public async Task<ActionResult<GetOffersResponse>> GetTopOfferByLocation([FromBody] GetByLocationAndCategoryDTO model)
        {
            try
            {
                var user = await HttpContext.GetUserFromContext(_individualUserRepository);

                if (user == null)
                {
                    return Unauthorized(new RedeemOfferResponse
                    (
                        statusCode: StatusCodes.Status401Unauthorized,
                        message: "Unauthorized: Invalid user",
                        payload: null
                    ));
                }

                var business = await _service.GetTopOfferByLocation(model, user);
                return Ok(business);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetOffersResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: null
                );
            }
        }

        [HttpGet]
        [Route("[action]")]
        [AuthPermission(Permissions.Offer.View)]
        public async Task<ActionResult<GetOffersResponse>> GetOffersByFilter([FromQuery] EntityFilter filter, [FromQuery] LocationDTO? location)
        {
            try
            {
                var business = await _service.GetOffersByFilter(filter, location);
                return Ok(business);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetOffersResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: null
                );
            }
        }

        [HttpGet]
        [Route("[action]")]
        [AuthPermission(Permissions.Offer.View)]
        [SwaggerOperation(Tags = new[] { "Mobile" })]
        public async Task<ActionResult<RedeemOfferResponse>> RedeemOffer([FromQuery] int offerId)
        {
            try
            {
                var user = await HttpContext.GetUserFromContext(_individualUserRepository);
                if (user == null)
                {
                    return Unauthorized(new RedeemOfferResponse
                    (
                        statusCode: StatusCodes.Status401Unauthorized,
                        message: "Unable to retrieve email from token.",
                        payload: null
                    ));
                }
                var result = await _service.RedeemOffer(user, offerId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new RedeemOfferResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: null
                );
            }
        }

        [HttpGet]
        [Route("[action]")]
        [AuthPermission(Permissions.Offer.View)]
        [SwaggerOperation(Tags = new[] { "Mobile" })]
        public async Task<ActionResult<UserRedeemOfferResponse>> GetRedeemOffers()
        {
            try
            {
                var user = await HttpContext.GetUserFromContext(_individualUserRepository);
                if (user == null)
                {
                    return Unauthorized(new RedeemOfferResponse
                    (
                        statusCode: StatusCodes.Status401Unauthorized,
                        message: "Unable to retrieve email from token.",
                        payload: null
                    ));
                }
                var result = await _service.GetRedeemOffers(user);
                return Ok(result);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new UserRedeemOfferResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: null
                );
            }
        }

        [HttpGet]
        [Route("[action]")]
        [AuthPermission(Permissions.Offer.View)]
        [SwaggerOperation(Tags = new[] { "Business" })]
        public async Task<ActionResult<OfferStatusResponse>> GetOfferStatus()
        {
            try
            {
                var userIdClaim = HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return new OfferStatusResponse(
                        StatusCodes.Status401Unauthorized,
                        "Unauthorized: Unable to extract user ID.",
                        null
                    );
                }

                if (!Guid.TryParse(userIdClaim.Value, out Guid userId))
                {
                    return new OfferStatusResponse(
                        StatusCodes.Status401Unauthorized,
                        "Unauthorized: Invalid user ID format.",
                        null
                    );
                }

                var offer = await _service.GetOfferStatus(userId);
                return Ok(offer);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new OfferStatusResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: null
                );
            }
        }

        [HttpPost]
        [Route("[action]")]
        [AuthPermission(Permissions.Offer.View)]
        [SwaggerOperation(Tags = new[] { "Mobile" })]
        public async Task<ActionResult<RedeemOfferQrResponse>> RedeemOfferByQrCode([FromBody] RedeemOfferByQrDTO model)
        {
            try
            {
                var user = await HttpContext.GetUserFromContext(_individualUserRepository);
                if (user == null)
                {
                    return Unauthorized(new RedeemOfferQrResponse
                    (
                        statusCode: StatusCodes.Status401Unauthorized,
                        message: "Unauthorized: Invalid user",
                        payload: null
                    ));
                }
                var result = await _service.RedeemOfferByQrCode(user, model);
                return Ok(result);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new RedeemOfferQrResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: null
                );
            }
        }
        
        [HttpGet]
        [Route("[action]")]
        [AuthPermission(Permissions.Offer.View)]
        [SwaggerOperation(Tags = new[] { "Mobile" })]
        public async Task<ActionResult<GetSearchFilters>> GetSearchFilter()
        {
            try
            {
                var user = await HttpContext.GetUserFromContext(_individualUserRepository);
                if (user == null)
                {
                    return Unauthorized(new GetSearchFilters
                    (
                        statusCode: StatusCodes.Status401Unauthorized,
                        message: "Unauthorized: Invalid user",
                        payload: null
                    ));
                }
                var result = await _service.GetSearchFilter();
                return Ok(result);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetSearchFilters
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: null
                );
            }
        }

        [HttpDelete]
        [Route("[action]")]
        [AuthPermission(Permissions.Offer.Delete)]
        [SwaggerOperation(Tags = new[] { "Dashboard", "Business"})]
        public async Task<ActionResult<SuccessResponse>> DeleteOffer([FromQuery] int offerId)
        {
            try
            {
                var response = await _service.DeleteOffer(offerId);
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
        [AuthPermission(Permissions.Offer.Create)]
        [SwaggerOperation(Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<GetBusinessForOffersResponse>> GetBusinessForOffers()
        {
            try
            {
                var business = await _service.GetBusinessForOffers();
                return Ok(business);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetBusinessForOffersResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: null
                );
            }
        }

    }
}