
using SaudiPlus.Entities;

namespace SaudiPlus.Interfaces.Repository
{
    public interface IBannerRepository : IRepository<Banner, int>
    {
        IQueryable<Banner> GetBannersForFilter();
        Task<Banner?> GetBannerWithRelations(int id);
        Task<List<Banner>> GetBannersWithRelations();
    }
}
