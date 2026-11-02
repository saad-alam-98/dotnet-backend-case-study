using SaudiPlus.Entities;

namespace SaudiPlus.Interfaces.Repository
{
    public interface IPartnerRepository : IRepository<Partner, int>
    {
        IQueryable<Partner> GetPartnersForFilter();
        Task<List<Partner>> GetPartnersWithRelation();
        Task<Partner?> GetPartnerWithRelation(int id);
        Task<Partner?> GetBusinessPartner(Guid businessId);
    }
}
