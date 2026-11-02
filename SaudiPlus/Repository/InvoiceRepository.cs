using SaudiPlus.Entities;
using Microsoft.EntityFrameworkCore;
using SaudiPlus.Interfaces.Repository;

namespace SaudiPlus.Repository
{
    public class InvoiceRepository : Repository<Invoice, int>, IInvoiceRepository
    {
        public InvoiceRepository(SaudiPlusDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<List<Invoice>> GetInvoiceByBusinessId(Guid businessId)
        {
            return await _dbContext.Invoice
               .Where(invoice => invoice.BusinessId == businessId && invoice.IsVerified)
               .ToListAsync();
        }

        public IQueryable<Invoice> GetUserInvoiceForFilter(Guid UserId)
        {
            return _dbContext.Invoice
                .Where(invoice => invoice.UserId == UserId && invoice.IsVerified)
                .Include(b => b.Business)
                .Include(b => b.Branch)
                .Include(b => b.User)
                .Include(b => b.Offer)
                .OrderByDescending(b => b.CreationTime)
                .AsQueryable();
        }

        public IQueryable<Invoice> GetInvoiceForFilter(Guid businessId)
        {
            return _dbContext.Invoice
                .Where(invoice => invoice.BusinessId == businessId && invoice.IsVerified)
                .Include(b => b.Business)
                .Include(b => b.Branch)
                .Include(b => b.User)
                .Include(b => b.Offer)
                .OrderByDescending(b => b.CreationTime)
                .AsQueryable();
        }

        public async Task<Invoice?> GetInvoiceWithUserRelation(int id)
        {
            return await _dbContext.Invoice
                .Include(b => b.User)
                .Include(b => b.Offer)
                .FirstOrDefaultAsync(invoice => invoice.Id == id);
        }

        public async Task<List<Invoice>> GetInvoicesByYear(int year)
        {
            return await _dbContext.Invoice
                .Where(i => i.IsVerified)
                .Where(i => i.CreationTime.Year == year)
                .OrderByDescending(b => b.CreationTime)
                .ToListAsync();
        }

        public IQueryable<Invoice> GetInvoiceByMonth(int month, int year)
        {
            return _dbContext.Invoice
                .Where(i => i.IsVerified)
                .Where(i => i.CreationTime.Year == year && i.CreationTime.Month == month)
                .Include(i => i.Business)
                .Include(i => i.Offer)
                .Include(i => i.User)
                .OrderByDescending(b => b.CreationTime)
                .AsQueryable();
        }
    }
}
