using SaudiPlus.Entities;
using SaudiPlus.Response;
using SaudiPlus.Helper.Filter;
using SaudiPlus.Request.Invoice;
using SaudiPlus.Response.Invoice;

namespace SaudiPlus.Interfaces.Service
{
    public interface IInvoiceService
    {
        Task<GetBranchForInvoice> GetBranchForInvoice(Guid businessUserId);
        Task<CreateInvoiceResponse> CreateInvoice(BusinessUser businessUser, CreateInvoiceDTO model);
        Task<SuccessResponse> SaveInvoice(BusinessUser businessUser, SaveInvoiceDTO model);
        Task<GetInvoicesResponse> GetInvoiceByFilter(Guid businessUserId, EntityFilter filter);
        Task<GetOfferForInvoiceResponse> GetBusinessInvoiceOffers(Guid businessUserId);
    }
}
