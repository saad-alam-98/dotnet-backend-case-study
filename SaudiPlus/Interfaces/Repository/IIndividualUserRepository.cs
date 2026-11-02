using SaudiPlus.Entities;

namespace SaudiPlus.Interfaces.Repository
{
    public interface IIndividualUserRepository : IRepository<IndividualUser, Guid>
    {
        IQueryable<IndividualUser> GetUsersForFilter();
        Task<IndividualUser?> GetUserWithRelationsByEmail(string email);
        Task<IndividualUser?> GetUserByMobileNumber(string MobileNumber);
        Task<IndividualUser?> GetUserSupportTicketByEmail(string email);
        Task<List<IndividualUser>> GetUserWithRelations();
        Task<IndividualUser?> GetUserWithRelations(Guid userId);
        Task<IndividualUser?> GetUserByUserId(Guid userId);
        Task<IndividualUser?> GetUserInvoice(Guid userId);
        int GetUsersCount();
    }
}
