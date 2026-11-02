using SaudiPlus.Entities;

namespace SaudiPlus.Interfaces.Repository
{
    public interface IOfferRepository : IRepository<Offer, int>
    {
        IQueryable<Offer> GetOffersForFilter(bool filterExpire);
        Task<List<Offer>> GetTopOfferWithRelations();
        Task<Offer?> GetOfferWithRelations(int id);
        Task<List<Offer>> GetBusinessOffers(Guid businessId, bool filterExpire);
        int GetTotalActiveOffers();
        int GetTotalActiveOffers(Guid businessId);
        Task<List<Offer>> GetOffersByMonth(int month, int year);
    }
}
