using SaudiPlus.Entities;

namespace SaudiPlus.Interfaces.Repository
{
    public interface ICategoryRepository : IRepository<Category, int>
    {
        IQueryable<Category> GetCategoriesForFilter();
        public Task<bool> IsCategoryAssigned(int categoryId);
    }
}
