using SaudiPlus.Request;
using SaudiPlus.Response;
using SaudiPlus.Constants;
using SaudiPlus.Extensions;
using SaudiPlus.Helper.Filter;
using SaudiPlus.Request.Offer;
using Microsoft.AspNetCore.Mvc;
using SaudiPlus.Request.Category;
using SaudiPlus.Response.Category;
using SaudiPlus.Interfaces.Service;
using SaudiPlus.Request.BusinessUser;
using SaudiPlus.Interfaces.Repository;
using SaudiPlus.Response.BusinessUser;
using Microsoft.AspNetCore.Authorization;
using Swashbuckle.AspNetCore.Annotations;

namespace SaudiPlus.Controllers.BusinessUser
{
    [ApiController]
    [Route("api/[controller]")]
    public class BusinessUserController : BaseController<BusinessUserController>
    {
        private readonly IOfferService _offerService;
        private readonly IBusinessUserService _service;
        private readonly IBusinessRepository _businessRepository;
        private readonly IIndividualUserRepository _individualUserRepository;
        public BusinessUserController(
            IOfferService offerService,
            IBusinessRepository businessRepository,
            IBusinessUserService businessUserService,
            IIndividualUserRepository individualUserRepository,
            ILogger<BusinessUserController> logger
        )
        : base(logger)
        {
            _service = businessUserService;
            _offerService = offerService;
            _businessRepository = businessRepository;
            _individualUserRepository = individualUserRepository;
        }

        [HttpPost]
        [Route("[action]")]
        [AllowAnonymous]
        [SwaggerOperation(Tags = new[] { "Mobile" })]
        public async Task<ActionResult<SuccessResponse>> CreateBusinessUser([FromBody] CreateBusinessDTO model)
        {
            try
            {
                var user = await _service.CreateBusinessUser(model);
                return Ok(user);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new SuccessResponse
                (
                    statusCode: StatusCodes.Status200OK,
                    message: ex.Message,
                    payload: new SuccessResponsePayload { success = false }
                );
            }
        }

        [HttpPost]
        [Route("[action]")]
        [AuthPermission(Permissions.Business.Create)]
        [SwaggerOperation(Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<SuccessResponse>> AdminCreateBusinessUser([FromForm] AdminCreateBusinessDTO model)
        {
            try
            {
                var user = await _service.AdminCreateBusinessUser(model);
                return Ok(user);
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
        [AuthPermission(Permissions.Business.Edit)]
        [SwaggerOperation(Tags = new[] { "Business" })]
        public async Task<ActionResult<SuccessResponse>> EditBusiness(EditBusinessDTO model)
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
                var response = await _service.EditBusiness(business.Id, model);
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
        [AuthPermission(Permissions.Business.Edit)]
        [SwaggerOperation(Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<SuccessResponse>> EditBusinessByAdmin(EditBusinessByAdmin model)
        {
            try
            {
                var response = await _service.EditBusiness(model.BusinessId, model);
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
        [AuthPermission(Permissions.Business.Edit)]
        [SwaggerOperation(Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<SuccessResponse>> ToggleBusinessStatus([FromQuery] ToggleStatusDTO<Guid> model)
        {
            try
            {
                var response = await _service.ToggleBusinessStatus(model);
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
        [AuthPermission(Permissions.Business.View)]
        [SwaggerOperation(Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<GetBusinessResponse>> AdminGetBusinessById(Guid businessId)
        {
            try
            {
                var business = await _service.GetBusinessByIdDetailed(businessId);
                return Ok(business);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetBusinessResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: null
                );
            }
        }

        [HttpGet]
        [Route("[action]")]
        [AuthPermission(Permissions.Business.View)]
        [SwaggerOperation(Tags = new[] { "Business" })]
        public async Task<ActionResult<GetBusinessResponse>> GetBusiness()
        {
            try
            {
                var business = await HttpContext.GetBusinessFromContext(_businessRepository);
                if (business == null)
                {
                    return new GetBusinessResponse(
                        StatusCodes.Status401Unauthorized,
                        "Unauthorized: Invalid business",
                        null
                    );
                }
                var response = await _service.GetBusinessByIdDetailed(business.Id);
                return Ok(response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetBusinessResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: null
                );
            }
        }

        [HttpGet]
        [Route("[action]")]
        [AuthPermission(Permissions.Business.View)]
        [SwaggerOperation(Tags = new[] { "Mobile" })]
        public async Task<ActionResult<GetBusinessResponse>> GetBusinessByIdLocalized(Guid businessId)
        {
            try
            {
                var business = await _service.GetBusinessByIdLocalized(businessId);
                return Ok(business);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetBusinessResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: null
                );
            }
        }
        
        [HttpGet]
        [Route("[action]")]
        [AuthPermission(Permissions.Business.View)]
        [SwaggerOperation(Tags = new[] { "Mobile" })]
        public async Task<ActionResult<GetBusinessDetailsById>> GetBusinessDetailsByBranch([FromQuery] int branchId, [FromQuery] LocationDTO? location)
        {
            try
            {
                var user = await HttpContext.GetUserFromContext(_individualUserRepository);
                if (user == null)
                {
                    return new GetBusinessDetailsById(
                        StatusCodes.Status401Unauthorized,
                        "Unauthorized: Invalid User",
                        null
                    );
                }

                var business = await _service.GetBusinessDetailsByBranch(user.Id, branchId, location);
                return Ok(business);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetBusinessDetailsById
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: null
                );
            }
        }

        [HttpGet]
        [Route("[action]")]
        [AuthPermission(Permissions.Business.View)]
        [SwaggerOperation(Tags = new[] { "Mobile" })]
        public async Task<ActionResult<GetBusinessDetailsById>> GetBusinessDetailsById(Guid businessId, [FromQuery] LocationDTO? location)
        {
            try
            {
                var user = await HttpContext.GetUserFromContext(_individualUserRepository);
                if (user == null)
                {
                    return new GetBusinessDetailsById(
                        StatusCodes.Status401Unauthorized,
                        "Unauthorized: Invalid User",
                        null
                    );
                }

                var business = await _service.GetBusinessDetailsById(user.Id, businessId, location);
                return Ok(business);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetBusinessDetailsById
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: null
                );
            }
        }

        [HttpGet]
        [Route("[action]")]
        [AuthPermission(Permissions.Business.View)]
        [SwaggerOperation(Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<GetBusinessesResponse>> GetAllBusiness()
        {
            try
            {
                var business = await _service.GetBusiness();
                return Ok(business);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetBusinessesResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: null
                );
            }
        }

        [HttpGet]
        [Route("[action]")]
        [AuthPermission(Permissions.Business.View)]
        [SwaggerOperation(Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<GetBusinessesResponse>> GetActiveBusiness()
        {
            try
            {
                var business = await _service.GetActiveBusiness();
                return Ok(business);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetBusinessesResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: null
                );
            }
        }

        [HttpGet]
        [Route("[action]")]
        [AuthPermission(Permissions.Business.View)]
        [SwaggerOperation(Tags = new[] { "Mobile" })]
        public async Task<ActionResult<GetBusinessesResponse>> GetActiveBusinessLocalized()
        {
            try
            {
                var business = await _service.GetActiveBusinessLocalized();
                return Ok(business);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetBusinessesResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: null
                );
            }
        }

        [HttpGet]
        [Route("[action]")]
        [AuthPermission(Permissions.Business.View)]
        [SwaggerOperation(Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<GetBusinessesResponse>> GetBusinessByFilter([FromQuery] EntityFilter filter)
        {
            try
            {
                var business = await _service.GetBusinessByFilter(filter);
                return Ok(business);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetBusinessesResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: null
                );
            }
        }

        [HttpGet]
        [Route("[action]")]
        [AuthPermission(Permissions.Business.View)]
        [SwaggerOperation(Tags = new[] { "Mobile" })]
        public async Task<ActionResult<GetBusinessesResponse>> GetBusinessByFilterLocalized([FromQuery] EntityFilter filter)
        {
            try
            {
                var business = await _service.GetBusinessByFilterLocalized(filter);
                return Ok(business);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetBusinessesResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: null
                );
            }
        }

        [HttpGet]
        [Route("[action]")]
        [AuthPermission(Permissions.Business.View)]
        [SwaggerOperation(Tags = new[] { "Mobile" })]
        public async Task<ActionResult<GetBusinessesResponse>> GetTopBusiness()
        {
            try
            {
                var business = await _service.GetTopBusinessLocalized();
                return Ok(business);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetBusinessesResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: null
                );
            }
        }

        [HttpPost]
        [Route("[action]")]
        [AuthPermission(Permissions.Business.Edit)]
        [SwaggerOperation(Tags = new[] { "Business" })]
        public async Task<ActionResult<SuccessResponse>> SetBusinessContact(BusinessContactDTO model)
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
                var response = await _service.SetBusinessContact(business, model);
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

        [HttpPost]
        [Route("[action]")]
        [AuthPermission(Permissions.Business.Edit)]
        [SwaggerOperation(Tags = new[] { "Business" })]
        public async Task<ActionResult<SuccessResponse>> SetBusinessSocialMedia(SetSocialMediaDto model)
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
                var response = await _service.SetBusinessSocialMedia(business, model);
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
        [AuthPermission(Permissions.Business.View)]
        [SwaggerOperation(Tags = new[] { "Business" })]
        public async Task<ActionResult<GetBusinessContactResponse>> GetBusinessContact()
        {
            try
            {
                var business = await HttpContext.GetBusinessFromContext(_businessRepository);
                if (business == null)
                {
                    return new GetBusinessContactResponse(
                        StatusCodes.Status401Unauthorized,
                        "Unauthorized: Invalid business",
                        null
                    );
                }
                var response = await _service.GetBusinessContact(business.Id);
                return Ok(response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetBusinessContactResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: null
                );
            }
        }

        [HttpGet]
        [Route("[action]")]
        [AuthPermission(Permissions.Business.View)]
        [SwaggerOperation(Tags = new[] { "Business" })]
        public async Task<ActionResult<BusinessStatsResponse>> GetBusinessStats()
        {
            try
            {
                var business = await HttpContext.GetBusinessFromContext(_businessRepository);
                if (business == null)
                {
                    return new BusinessStatsResponse(
                        StatusCodes.Status401Unauthorized,
                        "Unauthorized: Invalid business",
                        null
                    );
                }
                var response = await _service.GetBusinessStats(business.Id);
                return Ok(response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new BusinessStatsResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: null
                );
            }
        }

        [HttpGet]
        [Route("[action]")]
        [AuthPermission(Permissions.Business.View)]
        [SwaggerOperation(Tags = new[] { "Business" })]
        public async Task<ActionResult<GetBusinessSalesResponse>> GetMonthlySales([FromQuery] int? year)
        {
            try
            {
                var business = await HttpContext.GetBusinessFromContext(_businessRepository);
                if (business == null)
                {
                    return new GetBusinessSalesResponse(
                        StatusCodes.Status401Unauthorized,
                        "Unauthorized: Invalid business",
                        null
                    );
                }
                var response = await _service.GetMonthlySales(business.Id, year);
                return Ok(response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetBusinessSalesResponse
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
        public async Task<ActionResult<GetOfferRedeemStateResponse>> GetMonthlyOfferRedeemCount([FromQuery] int? year)
        {
            try
            {
                var business = await HttpContext.GetBusinessFromContext(_businessRepository);
                if (business == null)
                {
                    return new GetOfferRedeemStateResponse(
                        StatusCodes.Status401Unauthorized,
                        "Unauthorized: Invalid business",
                        null
                    );
                }
                var response = await _service.GetMonthlyOfferRedeemCount(business.Id, year);
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

        [HttpPost]
        [Route("[action]")]
        [AuthPermission(Permissions.Business.View)]
        [SwaggerOperation(Tags = new[] { "Mobile" })]
        public async Task<ActionResult<GetBusinessByLocationResponse>> GetBusinessByLocation([FromBody] GetByLocationAndCategoryDTO model)
        {
            try
            {
                var user = await HttpContext.GetUserFromContext(_individualUserRepository);
                if (user == null)
                {
                    return new GetBusinessByLocationResponse(
                        StatusCodes.Status401Unauthorized,
                        "Unauthorized: Invalid User",
                        null
                    );
                }

                var business = await _service.GetBusinessByLocation(user, model);
                return Ok(business);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetBusinessByLocationResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: null
                );
            }
        }

        [HttpPatch]
        [Route("[action]")]
        [AuthPermission(Permissions.Business.Edit)]
        [SwaggerOperation(Tags = new[] { "Business" })]
        public async Task<ActionResult<GetBusinessAppResponse>> SetBusinessAppLinks([FromBody] SetBusinessAppLinksDTO model)
        {
            try
            {
                var business = await HttpContext.GetBusinessFromContext(_businessRepository);
                if (business == null)
                {
                    return new GetBusinessAppResponse(
                       StatusCodes.Status401Unauthorized,
                       "Unauthorized: Invalid business",
                        null
                   );
                }
                var response = await _service.SetBusinessAppLinks(business, model);
                return Ok(response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetBusinessAppResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: null
                );
            }
        }

        [HttpPatch]
        [Route("[action]")]
        [AuthPermission(Permissions.Business.Edit)]
        [SwaggerOperation(Tags = new[] { "Business" })]
        public async Task<ActionResult<GetBusinessAppResponse>> SetBusinessWebiste([FromBody] SetWebsiteLinkDTO model)
        {
            try
            {
                var business = await HttpContext.GetBusinessFromContext(_businessRepository);
                if (business == null)
                {
                    return new GetBusinessAppResponse(
                       StatusCodes.Status401Unauthorized,
                       "Unauthorized: Invalid business",
                        null
                   );
                }
                var response = await _service.SetBusinessWebiste(business, model);
                return Ok(response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetBusinessAppResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: null
                );
            }
        }

        [HttpGet]
        [Route("[action]")]
        [AuthPermission(Permissions.Business.View)]
        [SwaggerOperation(Tags = new[] { "Business" })]
        public async Task<ActionResult<GetBusinessAppResponse>> GetWebiste()
        {
            try
            {
                var business = await HttpContext.GetBusinessFromContext(_businessRepository);
                if (business == null)
                {
                    return new GetBusinessAppResponse(
                       StatusCodes.Status401Unauthorized,
                       "Unauthorized: Invalid business",
                        null
                   );
                }
                var response = await _service.GetBusinessWebiste(business.Id);
                return Ok(response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetBusinessAppResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: null
                );
            }
        }

        [HttpGet]
        [Route("[action]")]
        [AuthPermission(Permissions.Business.View)]
        [SwaggerOperation(Tags = new[] { "Mobile" })]
        public async Task<ActionResult<GetBusinessAppResponse>> GetBusinessWebiste([FromQuery] Guid businessId)
        {
            try
            {
                var response = await _service.GetBusinessWebiste(businessId);
                return Ok(response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetBusinessAppResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: null
                );
            }
        }

        [HttpGet]
        [Route("[action]")]
        [AuthPermission(Permissions.Business.View)]
        [SwaggerOperation(Tags = new[] { "Business" })]
        public async Task<ActionResult<GetBusinessAppResponse>> GetAppLink()
        {
            try
            {
                var business = await HttpContext.GetBusinessFromContext(_businessRepository);
                if (business == null)
                {
                    return new GetBusinessAppResponse(
                       StatusCodes.Status401Unauthorized,
                       "Unauthorized: Invalid business",
                        null
                   );
                }
                var response = await _service.GetBusinessAppLinks(business.Id);
                return Ok(response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetBusinessAppResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: null
                );
            }
        }

        [HttpGet]
        [Route("[action]")]
        [AuthPermission(Permissions.Business.View)]
        [SwaggerOperation(Tags = new[] { "Mobile" })]
        public async Task<ActionResult<GetBusinessAppResponse>> GetBusinessAppLinks([FromQuery] Guid businessId)
        {
            try
            {
                var response = await _service.GetBusinessAppLinks(businessId);
                return Ok(response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetBusinessAppResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: null
                );
            }
        }

        [HttpPost]
        [Route("[action]")]
        [AuthPermission(Permissions.Business.View)]
        [SwaggerOperation(Tags = new[] { "Mobile" })]
        public async Task<ActionResult<GetFilterByTypeResponse>> GetItemByCategory([FromBody] TypeFilterDTO model)
        {
            try
            {
                GetFilterByTypeResponse response;
                var type = model.Type.Trim().ToLower();
                if (type == "business")
                {
                    response = await _service.GetItemByCategory(model);
                }
                else if (type == "offer")
                {
                    response = await _offerService.GetItemByCategory(model);
                }
                else
                {
                    response = new GetFilterByTypeResponse
                    (
                        statusCode: StatusCodes.Status500InternalServerError,
                        message: "Invalid Type",
                        payload: null
                    );
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetFilterByTypeResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: null
                );
            }
        }


        [HttpDelete]
        [Route("[action]")]
        [AuthPermission(Permissions.Business.Delete)]
        [SwaggerOperation(Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<SuccessResponse>> DeleteBusiness([FromQuery] Guid businessId)
        {
            try
            {
                var business = await _service.DeleteBusiness(businessId);
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
