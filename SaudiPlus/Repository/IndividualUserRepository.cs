using SaudiPlus.Entities;
using Microsoft.EntityFrameworkCore;
using SaudiPlus.Interfaces.Repository;

namespace SaudiPlus.Repository
{
    public class IndividualUserRepository : Repository<IndividualUser, Guid>, IIndividualUserRepository
    {
        public IndividualUserRepository(SaudiPlusDbContext context) : base(context) 
        { 
        }
    
        public async Task<IndividualUser?> GetUserWithRelations(Guid userId)
        {
            return await _dbContext.IndividualUser
                 .Include(u => u.City)
             .FirstOrDefaultAsync(u => u.Id == userId);
        }
        public async Task<IndividualUser?> GetUserWithRelationsByEmail(string email)
        {
            return await _dbContext.IndividualUser
                 .Include(u => u.City)
             .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<List<IndividualUser>> GetUserWithRelations()
        {
            return await _dbContext.IndividualUser
                .Include(u => u.City)
                .ToListAsync();
        }

        public IQueryable<IndividualUser> GetUsersForFilter()
        {
            return _dbContext.IndividualUser
              .Where(b => !b.IsDelete)
              .Include(u => u.City)
              .OrderByDescending(b => b.CreationTime)
              .AsQueryable();
        }

        public async Task<IndividualUser?> GetUserSupportTicketByEmail(string email)
        {
            return await _dbContext.IndividualUser
                 .Include(u => u.SupportTickets)
             .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<IndividualUser?> GetUserByMobileNumber(string MobileNumber)
        {
            return await _dbContext.IndividualUser
            .FirstOrDefaultAsync(u => u.PhoneNumber == MobileNumber);
        }

        public async Task<IndividualUser?> GetUserByUserId(Guid userId)
        {
            return await _dbContext.IndividualUser
                 .Include(u => u.City)
             .FirstOrDefaultAsync(u => u.UserId == userId);
        }

        public async Task<IndividualUser?> GetUserInvoice(Guid userId)
        {
            return await _dbContext.IndividualUser
               .Include(u => u.Invoice)
                .ThenInclude(i => i.Offer)
           .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public int GetUsersCount()
        {
            return _dbContext.IndividualUser
                .Where(b => b.IsActive && !b.IsDelete)
                .Count();
        }
    }
}
