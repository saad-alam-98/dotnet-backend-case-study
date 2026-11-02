using SaudiPlus.Enums;
using SaudiPlus.Entities;

namespace SaudiPlus.Interfaces.Repository
{
    public interface ISupportTicketRepository : IRepository<SupportTicket, int>
    {
        Task<SupportTicket?> GetSupportTicketWithRelation(int id);

        Task<(List<SupportTicket> Tickets, int TotalCount)> GetSupportTicketWithRelationPaginated(
            int pageSize,
            int pageNumber,
            string? searchKeyword = null,
            bool? isActive = null,
            TicketStatus[]? statuses = null );

        Task<(List<SupportTicket> Tickets, int TotalCount)> GetUserTicketsPaginated(
            Guid userId,
            int pageSize,
            int pageNumber,
            string? searchKeyword = null,
            bool? isActive = null,
            TicketStatus? status = null);

        Task<bool> HasOpenTickets(Guid userId);
        Task<bool> HasRelatedTickets(int supportTypeId);
    }
}
