using SaudiPlus.Enums;
using SaudiPlus.Entities;
using Microsoft.EntityFrameworkCore;
using SaudiPlus.Interfaces.Repository;

namespace SaudiPlus.Repository
{
    public class SupportTicketRepository : Repository<SupportTicket, int>, ISupportTicketRepository
    {
        public SupportTicketRepository(SaudiPlusDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<SupportTicket?> GetSupportTicketWithRelation(int id)
        {
            return await _dbContext.SupportTicket
                .Include(ticket => ticket.User)
                .Include(ticket => ticket.SupportType) 
                .Include(ticket => ticket.Messages)
                .FirstOrDefaultAsync(ticket => ticket.Id == id);
        }

        public async Task<(List<SupportTicket> Tickets, int TotalCount)> GetSupportTicketWithRelationPaginated(
            int pageSize,
            int pageNumber,
            string? searchKeyword = null,
            bool? isActive = null,
            TicketStatus[]? statuses = null)
        {
            var query = _dbContext.SupportTicket
                .Include(ticket => ticket.User)
                .Include(ticket => ticket.SupportType)
                .Include(ticket => ticket.Messages)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(searchKeyword))
            {
                searchKeyword = searchKeyword.ToLower();
                query = query.Where(ticket =>
                    ticket.User.DisplayName.ToLower().Contains(searchKeyword) ||
                    ticket.User.Email.ToLower().Contains(searchKeyword) ||
                    ticket.SupportType.NameEn.ToLower().Contains(searchKeyword) ||
                    ticket.SupportType.NameAr.ToLower().Contains(searchKeyword)
                );
            }

            if (isActive.HasValue)
            {
                query = query.Where(ticket => ticket.IsActive == isActive.Value);
            }

            if (statuses != null && statuses.Length > 0)
            {
                query = query.Where(ticket => statuses.Contains(ticket.TicketStatus));
            }

            query = query
                .OrderByDescending(ticket => ticket.TicketStatus == TicketStatus.New)
                .ThenByDescending(ticket => ticket.CreationTime);

            var totalCount = await query.CountAsync();

            var tickets = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (tickets, totalCount);
        }

        public async Task<(List<SupportTicket> Tickets, int TotalCount)> GetUserTicketsPaginated(
            Guid userId,
            int pageSize,
            int pageNumber,
            string? searchKeyword = null,
            bool? isActive = null,
            TicketStatus? status = null)
        {
            var query = _dbContext.SupportTicket
                .Include(ticket => ticket.User)
                .Include(ticket => ticket.SupportType)
                .Include(ticket => ticket.Messages)
                .Where(ticket => ticket.UserId == userId);

            // Apply filters
            if (!string.IsNullOrEmpty(searchKeyword))
            {
                searchKeyword = searchKeyword.ToLower();
                query = query.Where(ticket =>
                    ticket.SupportType.NameEn.ToLower().Contains(searchKeyword) ||
                    ticket.SupportType.NameAr.ToLower().Contains(searchKeyword)
                );
            }

            if (isActive.HasValue)
            {
                query = query.Where(ticket => ticket.IsActive == isActive.Value);
            }

            if (status.HasValue)
            {
                query = query.Where(ticket => ticket.TicketStatus == status.Value);
            }

            // Get total count for pagination
            var totalCount = await query.CountAsync();

            // Apply pagination
            var tickets = await query
                .OrderByDescending(ticket => ticket.CreationTime)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (tickets, totalCount);
        }

        public async Task<bool> HasOpenTickets(Guid userId)
        {
            var count = await _dbContext.SupportTicket
                .Where(t => 
                    t.UserId == userId && 
                    t.IsActive && 
                    (t.TicketStatus == TicketStatus.New || t.TicketStatus == TicketStatus.Open)
                )
                .CountAsync();
            
            return count > 0;
        }

        public async Task<bool> HasRelatedTickets(int supportTypeId)
        {
            var count = await _dbContext.SupportTicket
                .Where(ticket => ticket.SupportTypeId == supportTypeId)
                .CountAsync();
            
            return count > 0;
        }
    }
}