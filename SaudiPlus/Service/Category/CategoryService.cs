using AutoMapper;
using SaudiPlus.Request;
using SaudiPlus.Response;
using SaudiPlus.Interfaces;
using SaudiPlus.Helper.Filter;
using SaudiPlus.Helper.Bucket;
using SaudiPlus.Request.Category;
using SaudiPlus.Response.Category;
using Microsoft.Extensions.Options;
using SaudiPlus.Interfaces.Service;
using Microsoft.EntityFrameworkCore;
using SaudiPlus.Interfaces.Repository;

namespace SaudiPlus.Service.Category
{
    public class CategoryService : BaseService<CategoryService>, ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IFileService _fileService;
        private readonly BucketSetting _bucket;

        public CategoryService(
            ICategoryRepository categoryRepository,
            IFileService fileService,
            IOptions<BucketSetting> bucket,
            ILanguageService languageService,
            ILogger<CategoryService> logger,
            IMapper mapper
        ) : base(mapper, logger, languageService)
        {
            _categoryRepository = categoryRepository;
            _fileService = fileService;
            _bucket = bucket.Value;
            _mapper = mapper;
        }

        public async Task<CreateCateogryResponse> CreateCategory(CategoryDTO model)
        {
            var uploadResponse = await _fileService.UploadFileAsync(model.icon, _bucket.Category + "/" + model.NameEn);

            if (uploadResponse.error != null)
            {
                return new CreateCateogryResponse
                (
                    statusCode: StatusCodes.Status400BadRequest,
                    message: uploadResponse.error.Message,
                    payload: null
                );
            }

            try
            {
                var category = _mapper.Map<Entities.Category>(model);
                category.ImagePath = uploadResponse.URL;
                category.IsActive = true;

                await _categoryRepository.InsertAsync(category, autoSave: true);
                var payload = _mapper.Map<CategoryInfoDetailed>(category);

                return new CreateCateogryResponse
                (
                    StatusCodes.Status200OK,
                    "Category Created",
                    payload
                );
            }
            catch (Exception ex)
            {
                // Check if it's a unique constraint violation
                if (ex.InnerException != null && 
                   (ex.InnerException.Message.Contains("IX_Category_NameEn") || 
                    ex.InnerException.Message.Contains("IX_Category_NameAr")))
                {
                    return new CreateCateogryResponse
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: "A category with this name already exists",
                        payload: null
                    );
                }

                return new CreateCateogryResponse
                (
                    StatusCodes.Status500InternalServerError,
                    ex.Message,
                    null
                );
            }
        }

        public async Task<CreateCateogryResponse> EditCategory(EditCategoryDTO model)
        {
            var existingCategory = await _categoryRepository.GetByIdAsync(model.Id);
            if (existingCategory == null)
            {
                return new CreateCateogryResponse(
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "Category not found",
                    payload: null
                );
            }

            try
            {
                (string URL, Exception? error) uploadResponse = ("", null);
                if (model.icon != null)
                {
                    if (!string.IsNullOrEmpty(existingCategory.ImagePath))
                    {
                        await _fileService.DeleteFileAsync(existingCategory.ImagePath);
                    }

                    uploadResponse = await _fileService.UploadFileAsync(model.icon, _bucket.Category + "/" + existingCategory.NameEn);

                    if (uploadResponse.error != null)
                    {
                        return new CreateCateogryResponse
                        (
                            statusCode: StatusCodes.Status400BadRequest,
                            message: uploadResponse.error.Message,
                            payload: null
                        );
                    }
                }

                model.IsActive = model.IsActive.HasValue ? model.IsActive.Value : existingCategory.IsActive;
                _mapper.Map(model, existingCategory);

                if (!string.IsNullOrEmpty(uploadResponse.URL))
                {
                    existingCategory.ImagePath = uploadResponse.URL;
                }

                await _categoryRepository.UpdateAsync(existingCategory, true);
                var payload = _mapper.Map<CategoryInfoDetailed>(existingCategory);

                return new CreateCateogryResponse(
                    statusCode: StatusCodes.Status200OK,
                    message: "Category Updated",
                    payload: payload
                );
            }
            catch (Exception ex)
            {
                return new CreateCateogryResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: null
                );
            }
        }

        public async Task<GetCategoryResponse> GetCategoryById(int categoryId)
        {
            try
            {
                var response = await _categoryRepository.GetByIdAsync(categoryId);
                if (response == null)
                {
                    return new GetCategoryResponse(
                        statusCode: StatusCodes.Status500InternalServerError,
                        message: "Category not found",
                        payload: null
                    );
                }

                var item = _mapper.Map<CategoryInfoDetailed>(response);

                return new GetCategoryResponse
                (
                    statusCode: StatusCodes.Status200OK,
                    message: "Category Get Success",
                    payload: item
                );
            }
            catch (Exception ex)
            {
                return new GetCategoryResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: null
                );
            }
        }

        public async Task<GetCategoryResponse> GetCategoryByIdLocalized(int categoryId)
        {
            try
            {
                var (languageCode, error) = _languageService.GetLanguageCode();
                if (error != null)
                {
                    return new GetCategoryResponse
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: error.Message,
                        payload: null
                    );
                }

                var response = await _categoryRepository.GetByIdAsync(categoryId);
                if (response == null)
                {
                    return new GetCategoryResponse(
                        statusCode: StatusCodes.Status500InternalServerError,
                        message: "Category not found",
                        payload: null
                    );
                }

                var item = _mapper.Map<CategoryInfoDetailed>(response);

                return new GetCategoryResponse
                (
                    statusCode: StatusCodes.Status200OK,
                    message: "Category Get Success",
                    payload: item
                );
            }
            catch (Exception ex)
            {
                return new GetCategoryResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: null
                );
            }
        }

        public async Task<GetCategoriesResponse> GetActiveCategory()
        {
            try
            {
                var response = (await _categoryRepository.GetAsync())
                     .Where(r => r.IsActive);

                var payload = new GetCategoryPayload()
                {
                    Items = [],
                    TotalItems = response.Count(),
                    HasNextPage = false
                };
                foreach (var r in response)
                {
                    var item = _mapper.Map<CategoryInfoDetailed>(r);
                    payload.Items.Add(item);
                }

                return new GetCategoriesResponse
                (
                    StatusCodes.Status200OK,
                    "Fetch all active category success",
                    payload
                );
            }
            catch (Exception ex)
            {
                return new GetCategoriesResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: null
                );
            }
        }

        public async Task<GetCategoriesResponse> GetActiveCategoryLocalized()
        {
            try
            {
                var (languageCode, error) = _languageService.GetLanguageCode();
                if (error != null)
                {
                    return new GetCategoriesResponse
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: error.Message,
                        payload: null
                    );
                }

                var response = (await _categoryRepository.GetAsync())
                     .Where(r => r.IsActive);

                var payload = new GetCategoryPayload()
                {
                    Items = [],
                    TotalItems = response.Count(),
                    HasNextPage = false
                };
                foreach (var r in response)
                {
                    var item = _mapper.Map<CategoryInfoLocalized>(r);
                    item.Name = r.GetNameByLanguageCode(languageCode);
                    payload.Items.Add(item);
                }

                return new GetCategoriesResponse
                (
                    statusCode: StatusCodes.Status200OK,
                    message: "Fetch all localized active category success",
                    payload: payload
                );
            }
            catch (Exception ex)
            {
                return new GetCategoriesResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: null
                );
            }
        }

        public async Task<GetCategoriesResponse> GetCategories()
        {
            try
            {
                var response = (await _categoryRepository.GetAsync());

                var payload = new GetCategoryPayload()
                {
                    Items = [],
                    TotalItems = response.Count(),
                    HasNextPage = false
                };
                foreach (var r in response)
                {
                    var item = _mapper.Map<CategoryInfoDetailed>(r);
                    payload.Items.Add(item);
                }

                return new GetCategoriesResponse
                (
                    StatusCodes.Status200OK,
                    "Fetch All Category Success",
                    payload
                );
            }
            catch (Exception ex)
            {
                return new GetCategoriesResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: null
                );
            }
        }

        public async Task<SuccessResponse> ToggleCategoryStatus(ToggleStatusDTO<int> model)
        {
            try
            {
                var response = await _categoryRepository.GetByIdAsync(model.Id);
                if (response == null)
                {
                    return new SuccessResponse
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: $"Category with Id {model.Id} not found",
                        payload: new SuccessResponsePayload { success = false }
                    );
                }

                response.IsActive = model.IsActive;
                await _categoryRepository.UpdateAsync(response, true);

                return new SuccessResponse
                (
                    statusCode: StatusCodes.Status200OK,
                    message: "Category",
                    payload: new SuccessResponsePayload { success = true }
                );
            }
            catch (Exception ex)
            {
                return new SuccessResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: new SuccessResponsePayload { success = false }
                );
            }
        }

        public async Task<SuccessResponse> DeleteCategory(int categoryId)
        {
            try
            {
                var category = await _categoryRepository.GetByIdAsync(categoryId);
                if (category == null)
                {
                    return new SuccessResponse
                    (
                        statusCode: StatusCodes.Status404NotFound,
                        message: "Category not found",
                        payload: new SuccessResponsePayload { success = false }
                    );
                }

                var isCategoryAssigned = await _categoryRepository.IsCategoryAssigned(categoryId);
                if (isCategoryAssigned)
                {
                    return new SuccessResponse
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: "This category cannot be deleted because it is in use.",
                        payload: new SuccessResponsePayload { success = false }
                    );
                }

                if (!string.IsNullOrEmpty(category.ImagePath))
                {
                    await _fileService.DeleteFileAsync(category.ImagePath);
                }

                await _categoryRepository.DeleteAsync(categoryId, true);
                return new SuccessResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "Category Deleted",
                    payload: new SuccessResponsePayload { success = true }
                );
            }
            catch (Exception ex)
            {
                return new SuccessResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: new SuccessResponsePayload { success = false }
                );
            }
        }

        public async Task<GetCategoriesResponse> GetCategoriesByFilter(EntityFilter filter)
        {
            try
            {
                var (languageCode, languageError) = _languageService.GetLanguageCode();
                if (languageError != null)
                {
                    return new GetCategoriesResponse
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: languageError.Message,
                        payload: null
                    );
                }

                var query = _categoryRepository.GetCategoriesForFilter();
                if (filter.isActive.HasValue)
                {
                    query = query
                        .Where(category => category.IsActive == filter.isActive.Value);
                }

                if (!string.IsNullOrEmpty(filter.searchKeyword))
                {
                    var searchKeyword = filter.searchKeyword.ToLower();
                    query = query.Where(banner =>
                        banner.NameEn.ToLower().Contains(searchKeyword) ||
                        banner.NameAr.ToLower().Contains(searchKeyword));
                }

                var totalItems = await query.CountAsync();
                var categories = await query
                    .Skip((filter.pageNumber - 1) * filter.pageSize)
                    .Take(filter.pageSize)
                    .ToListAsync();

                bool hasNextPage = (filter.pageNumber * filter.pageSize) < totalItems;

                var payload = new GetCategoryPayload
                {
                    Items = [],
                    TotalItems = totalItems,
                    HasNextPage = hasNextPage
                };

                foreach (var category in categories)
                {
                    var item = _mapper.Map<CategoryInfoDetailed>(category);
                    payload.Items.Add(item);
                }

                return new GetCategoriesResponse
                (
                    statusCode: StatusCodes.Status200OK,
                    message: "Users Fetch Success",
                    payload: payload
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching categories");
                return new GetCategoriesResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: null
                );
            }
        }
    }
}
