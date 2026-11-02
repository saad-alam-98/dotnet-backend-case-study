using SaudiPlus.Entities;
using Microsoft.EntityFrameworkCore;
using SaudiPlus.Interfaces.Repository;

namespace SaudiPlus.Repository
{
    public class CategoryRepository : Repository<Category, int>, ICategoryRepository
    {
        public CategoryRepository(SaudiPlusDbContext dbContext) : base(dbContext)
        {
        }

        public IQueryable<Category> GetCategoriesForFilter()
        {
            return _dbContext.Category
                .OrderByDescending(b => b.CreationTime)
                .AsQueryable();
        }

        public async Task<bool> IsCategoryAssigned(int categoryId)
        {
            return await _dbContext.BusinessUsers.
                AnyAsync(business => business.CategoryId == categoryId);
        }
    }
}
