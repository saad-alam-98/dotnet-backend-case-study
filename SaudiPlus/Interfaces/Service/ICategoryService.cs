using SaudiPlus.Request;
using SaudiPlus.Response;
using SaudiPlus.Request.Category;
using SaudiPlus.Response.Category;
using SaudiPlus.Helper.Filter;
using SaudiPlus.Response.Banner;

namespace SaudiPlus.Interfaces.Service
{
    public interface ICategoryService
    {
        Task<CreateCateogryResponse> CreateCategory(CategoryDTO model);
        Task<CreateCateogryResponse> EditCategory(EditCategoryDTO model);

        Task<GetCategoryResponse> GetCategoryById(int categoryId);
        Task<GetCategoryResponse> GetCategoryByIdLocalized(int categoryId);
        
        Task<GetCategoriesResponse> GetActiveCategory();
        Task<GetCategoriesResponse> GetActiveCategoryLocalized();

        Task<GetCategoriesResponse> GetCategories();
        Task<GetCategoriesResponse> GetCategoriesByFilter(EntityFilter filter);
        Task<SuccessResponse> ToggleCategoryStatus(ToggleStatusDTO<int> model);
        Task<SuccessResponse> DeleteCategory(int categoryId);
    }
}
