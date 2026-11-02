using SaudiPlus.Request;
using SaudiPlus.Response;
using SaudiPlus.Constants;
using SaudiPlus.Helper.Filter;
using Microsoft.AspNetCore.Mvc;
using SaudiPlus.Request.Category;
using SaudiPlus.Response.Category;
using SaudiPlus.Interfaces.Service;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.AspNetCore.Authorization;

namespace SaudiPlus.Controllers.Category
{
    [Controller]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class CategoryController : BaseController<CategoryController>
    {
        private readonly ICategoryService _service;
        public CategoryController(
            ILogger<CategoryController> logger,
            ICategoryService categoryService
        ) : base(logger)
        {
            _service = categoryService;
        }

        [HttpPost]
        [Route("[action]")]
        [AuthPermission(Permissions.Category.Create)]
        [SwaggerOperation(Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<CreateCateogryResponse>> CreateCategory([FromForm] CategoryDTO model)
        {
            try
            {
                var result = await _service.CreateCategory(model);
                return Ok(result);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new CreateCateogryResponse
                (
                    statusCode: StatusCodes.Status200OK,
                    message: ex.Message,
                    payload: null
                );
            }
        }

        [HttpPatch]
        [Route("[action]")]
        [AuthPermission(Permissions.Category.Edit)]
        [SwaggerOperation(Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<CreateCateogryResponse>> EditCategory([FromForm] EditCategoryDTO model)
        {
            try
            {
                var response = await _service.EditCategory(model);
                return Ok(response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new CreateCateogryResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: null
                );
            }
        }

        [HttpGet]
        [Route("[action]")]
        [AuthPermission(Permissions.Category.View)]
        [SwaggerOperation(Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<GetCategoriesResponse>> GetActiveCategory()
        {
            try
            {
                var Response = await _service.GetActiveCategory();
                return Ok(Response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetCategoriesResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: null
                );
            }
        }

        [HttpGet]
        [Route("[action]")]
        [AuthPermission(Permissions.Category.View)]
        public async Task<ActionResult<GetCategoriesResponse>> GetActiveCategoryLocalized()
        {
            try
            {
                var Response = await _service.GetActiveCategoryLocalized();
                return Ok(Response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetCategoriesResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: null
                );
            }
        }

        [HttpGet]
        [Route("[action]")]
        [AuthPermission(Permissions.Category.View)]
        [SwaggerOperation(Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<GetCategoryResponse>> GetCategoryById([FromQuery] int categoryId)
        {
            try
            {
                var Response = await _service.GetCategoryById(categoryId);
                return Ok(Response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetCategoryResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: null
                );
            }
        }

        [HttpGet]
        [Route("[action]")]
        [AuthPermission(Permissions.Category.View)]
        [SwaggerOperation(Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<GetCategoryResponse>> GetCategoryByIdLocalized([FromQuery] int categoryId)
        {
            try
            {
                var Response = await _service.GetCategoryByIdLocalized(categoryId);
                return Ok(Response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetCategoryResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: null
                );
            }
        }

        [HttpGet]
        [Route("[action]")]
        [AuthPermission(Permissions.Category.View)]
        [SwaggerOperation(Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<GetCategoriesResponse>> GetCategories()
        {
            try
            {
                var Response = await _service.GetCategories();
                return Ok(Response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetCategoriesResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: null
                );
            }
        }

        [HttpGet]
        [Route("[action]")]
        [AuthPermission(Permissions.Category.View)]
        [SwaggerOperation(Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<GetCategoriesResponse>> GetCategoriesByFilter([FromQuery] EntityFilter filter)
        {
            try
            {
                var business = await _service.GetCategoriesByFilter(filter);
                return Ok(business);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new GetCategoriesResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: null
                );
            }
        }

        [HttpPatch]
        [Route("[action]")]
        [AuthPermission(Permissions.Category.Edit)]
        [SwaggerOperation(Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<SuccessResponse>> ToggleCateogryStatus([FromBody] ToggleStatusDTO<int> model)
        {
            try
            {
                var response = await _service.ToggleCategoryStatus(model);
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
        [AuthPermission(Permissions.Category.Delete)]
        [SwaggerOperation(Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<SuccessResponse>> DeleteCategory([FromQuery] int categoryId)
        {
            try
            {
                var response = await _service.DeleteCategory(categoryId);
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
