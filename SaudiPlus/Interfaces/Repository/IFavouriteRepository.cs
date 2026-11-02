using SaudiPlus.Entities;

namespace SaudiPlus.Interfaces.Repository
{
    public interface IFavouriteRepository : IRepository<Favourite, int>
    {
        Task<Favourite?> GetFavourite(Guid userId, Guid businessUserId);
        Task<bool> IsFavourite(Guid userId, Guid businessUserId);
        Task<List<BusinessUser>> GetFavouriteBusiness(Guid userId);
    }
}

