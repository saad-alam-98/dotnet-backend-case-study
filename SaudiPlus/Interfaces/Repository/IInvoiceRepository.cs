using SaudiPlus.Entities;

namespace SaudiPlus.Interfaces.Repository
{
    public interface IInvoiceRepository : IRepository<Invoice, int>
    {
        IQueryable<Invoice> GetUserInvoiceForFilter(Guid UserId);
        IQueryable<Invoice> GetInvoiceForFilter(Guid businessId);
        Task<List<Invoice>> GetInvoicesByYear(int year);
        Task<List<Invoice>> GetInvoiceByBusinessId(Guid businessId);
        Task<Invoice?> GetInvoiceWithUserRelation(int id);
        IQueryable<Invoice> GetInvoiceByMonth(int month, int year);
    }
}
