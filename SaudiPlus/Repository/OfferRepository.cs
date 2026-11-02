using SaudiPlus.Entities;
using Microsoft.EntityFrameworkCore;
using SaudiPlus.Interfaces.Repository;

namespace SaudiPlus.Repository
{
    public class OfferRepository : Repository<Offer, int>, IOfferRepository
    {
        public OfferRepository(SaudiPlusDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<Offer?> GetOfferWithRelations(int id)
        {
            return await _dbContext.Offers
                .Include(o => o.Business)
                    .ThenInclude(b => b.Branches)
                .Include(o => o.Business.Category)
                .Include(o => o.Logos)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<List<Offer>> GetBusinessOffers(Guid businessId, bool filterExpire)
        {
            return await _dbContext.Offers
                .Where(o => o.BusinessId == businessId)
                .Where(o => !filterExpire || o.EndDate > DateTime.UtcNow)
                .Include(o => o.Business)
                    .ThenInclude(b => b.Branches)
                .Include(o => o.Business.Category)
                .Include(o => o.Logos)
                .ToListAsync();
        }

        public IQueryable<Offer> GetOffersForFilter(bool filterExpire)
        {
            return _dbContext.Offers
                .Where(o => !filterExpire || o.EndDate > DateTime.UtcNow)
                .Include(o => o.Business)
                    .ThenInclude(b => b.Branches)
                .Include(o => o.Business.Category)
                .Include(o => o.Logos)
                .OrderByDescending(b => b.CreationTime)
                .AsQueryable();
        }

        public async Task<List<Offer>> GetTopOfferWithRelations()
        {
            return await _dbContext.Offers
             .Where(o => o.EndDate > DateTime.UtcNow)
             .Include(o => o.Business)
                .ThenInclude(b => b.Branches)
             .Include(o => o.Business.Category)
             .Include(o => o.Logos)
             .ToListAsync();
        }

        public int GetTotalActiveOffers()
        {
            return _dbContext.Offers
                .Where(o => o.IsActive)
                .Where(o => o.EndDate > DateTime.UtcNow && o.IsActive)
                .Count();
        }

        public int GetTotalActiveOffers(Guid businessId)
        {
            return _dbContext.Offers
                .Where(o => o.IsActive)
                .Where(o => o.BusinessId == businessId)
                .Where(o => o.EndDate > DateTime.UtcNow && o.IsActive)
                .Count();
        }
        public async Task<List<Offer>> GetOffersByMonth(int month, int year)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1); // Last day of the month
            
            return await _dbContext.Offers
                .Where(o => 
                    // Offers created in the specified month and year
                    (o.CreationTime.Year == year && o.CreationTime.Month == month) ||
                    // Or offers active during any part of that month
                    (o.StartDate <= endDate && o.EndDate >= startDate))
                .Include(o => o.Business)
                .ToListAsync();
        }
    }
}

