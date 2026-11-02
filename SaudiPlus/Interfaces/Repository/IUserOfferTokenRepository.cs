using SaudiPlus.Entities;

namespace SaudiPlus.Interfaces.Repository
{
    public interface IUserOfferTokenRepository : IRepository<UserOfferToken, int>
    {
        Task<UserOfferToken?> GetByUserId(Guid userId);
        Task<UserOfferToken?> GetUserTokenWithRelation(string code);
    }
}