using SaudiPlus.Entities;

namespace SaudiPlus.Interfaces.Repository
{
    public interface IUserOfferRedeemRepository : IRepository<UserOfferRedeem, int>
    {
        Task<UserOfferRedeem?> GetByOfferAndUserId(Guid userId, int offerId);
        Task<List<Offer>> GetUserRedeemedOffers(Guid userId);
        Task<List<UserOfferRedeem>> GetRedeemsByYear(int year);
        Task<List<UserOfferRedeem>> GetRedeemsByMonth(int year, int month);
    }
}
