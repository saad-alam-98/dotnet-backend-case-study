using SaudiPlus.Request;
using SaudiPlus.Response;
using SaudiPlus.Entities;
using SaudiPlus.Helper.Filter;
using SaudiPlus.Request.Support;
using SaudiPlus.Response.Support;

namespace SaudiPlus.Interfaces.Service
{
    public interface ISupportService
    {
        Task<SuccessResponse> CreateSupportType(AddSupportTypeDTO model);
        Task<SuccessResponse> EditSupportType(EditSupportTypeDTO model);
        Task<SuccessResponse> ToggleSupportType(ToggleStatusDTO<int> model);
        Task<GetSupportTypes> GetSupportTypes(EntityFilter filter);
        Task<GetSupportTypes> GetActiveSupportTypes();
        Task<CreateTicketResponse> CreateSupportTicket(IndividualUser user, CreateSupportTicketDTO model);
        Task<GetTicketsResponse> GetOpenTickets(EntityFilter filter);
        Task<GetTicketsResponse> GetClosedTickets(EntityFilter filter);
        Task<GetTicketsResponse> GetCurrentTicket(IndividualUser user);
        Task<SuccessResponse> ReplyTicket(bool isAdmin, ReplyTicketDTO model);
        Task<SuccessResponse> CloseSupportTicket(int ticketId);
        Task<SuccessResponse> DeleteSupportTicket(int ticketId);
        Task<SuccessResponse> DeleteSupportType(int typeId);
    }
}
