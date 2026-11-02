using SaudiPlus.Entities;
using Microsoft.EntityFrameworkCore;
using SaudiPlus.Interfaces.Repository;

namespace SaudiPlus.Repository
{
    public class StatusInteractionRepository : Repository<StatusInteraction, int>, IStatusInteractionRepository
    {
        public StatusInteractionRepository(SaudiPlusDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<StatusInteraction?> GetByStatusAndUserAsync(int statusId, Guid userId)
        {
            return await _dbContext.StatusInteraction
                .Include(s => s.IndividualUser)
                .Include(s => s.Status)
                .FirstOrDefaultAsync(x => x.StatusId == statusId && x.IndividualUserId == userId);
        }

        public async Task<List<StatusInteraction>> GetStatusInteractions(int statusId)
        {
            return await _dbContext.StatusInteraction
                .Where(s =>  s.StatusId == statusId)
                .ToListAsync();
        }
    }
}
