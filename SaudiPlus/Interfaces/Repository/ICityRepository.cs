using SaudiPlus.Entities;

namespace SaudiPlus.Interfaces.Repository
{
    public interface ICityRepository :IRepository<City, int>
    {
        IQueryable<City> GetCititesForFilter();
        Task<City?> GetCityWithBanners(int cityId);
        public Task<bool> IsCityAssigned(int cityId);
    }
}
